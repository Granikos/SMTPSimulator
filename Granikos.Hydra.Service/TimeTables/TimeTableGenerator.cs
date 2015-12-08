using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Granikos.Hydra.Core;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;
using Granikos.Hydra.Service.PriorityQueue;
using Granikos.Hydra.Service.Providers;
using Granikos.Hydra.SmtpClient;
using log4net;

namespace Granikos.Hydra.Service.TimeTables
{
    public class TimeTableGenerator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TimeTableGenerator));
        private readonly object _lockObject = new object();
        private readonly Random _random = new Random();

        private readonly DelayedQueue<SendableMail> _queue;
        private readonly ITimeTable _timeTable;
        private readonly ITimeTableType _timeTableType;

        private const string EICAR = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

        private string EmlFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["EmlFolder"];
                return logFolder != null? Path.Combine(folder, logFolder) : null;
            }
        }

        [Import]
        private IExternalMailboxGroupProvider _externalGroups;

        [Import]
        private IExternalUserProvider _externalUsers;

        [Import]
        private ILocalMailboxGroupProvider _localGroups;

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private ITimeTableProvider _timeTables;

        [Import]
        private IMailTemplateProvider _mailTemplates;

        [Import]
        private IAttachmentProvider _attachments;

        private Timer _timer;

        public TimeTableGenerator(ITimeTable timeTable, DelayedQueue<SendableMail> queue, CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(timeTable != null, "timeTable");
            Contract.Requires<ArgumentNullException>(queue != null, "queue");
            Contract.Requires<ArgumentNullException>(container != null, "container");

            _timeTable = timeTable;
            _queue = queue;

            _timeTableType = container.GetExport<ITimeTableType>(timeTable.Type).Value;
            _timeTableType.Parameters = _timeTable.Parameters;
            _timeTableType.Initialize();

            container.SatisfyImportsOnce(this);
        }

        public bool IsRunning
        {
            get { return _timer != null; }
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_timer == null)
                {
                    if (_timeTable.ProtocolLevel > ProtocolLevel.Off)
                    {
                        Logger.Info(string.Format("Timetable '{0}' was started.", _timeTable.Name));
                    }
                    _timer = new Timer(Callback, null, GetWaitTime(), TimeSpan.Zero);
                }
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_timer != null)
                {
                    if (_timeTable.ProtocolLevel > ProtocolLevel.Off)
                    {
                        Logger.Info(string.Format("Timetable '{0}' was stopped.", _timeTable.Name));
                    }
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        private void Callback(object state)
        {
            try
            {
                var from = GetFrom();
                var to = GetRecipients();
                var fromMail = new MailAddress(from);
                var toMail = to.Select(r => new MailAddress(r)).ToArray();
                var content = CreateContent(fromMail, toMail);
                var parsed = new Mail(fromMail, toMail, content.ToString());

                var sendableMail = new SendableMail(parsed, null)
                {
                    ResultHandler = success =>
                    {
                        if (success)
                            _timeTables.IncreaseSuccessMailCount(_timeTable.Id);
                        else
                        {
                            _timeTables.IncreaseErrorMailCount(_timeTable.Id);
                        }
                    }
                };

                if (_timeTable.ProtocolLevel > ProtocolLevel.Off)
                {
                    Logger.InfoFormat("Timetable '{0}' enqueued a mail.", _timeTable.Name);

                    if (_timeTable.ProtocolLevel == ProtocolLevel.Verbose)
                    {
                        Logger.InfoFormat("From: {0}, To: {1}", from, string.Join(", ", to));
                    }
                }

                _queue.Enqueue(sendableMail, TimeSpan.Zero);
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Timetable '{0}': An exception occured while sending a mail.", _timeTable.Name), e);
            }
            finally
            {
                _timer.Change(GetWaitTime(), TimeSpan.Zero);
            }
        }

        private MailContent CreateContent(MailAddress from, MailAddress[] to)
        {
            var template = _mailTemplates.GetMailTemplates().First(t => t.Id == _timeTable.MailTemplateId);

            var html = ReplaceTokens(template.Html, template.Title);
            var text = ReplaceTokens(template.Text, template.Title);

            var mc = new MailContent(template.Subject, from, html, text)
            {
                HeaderEncoding = GetEncoding(template.HeaderEncoding),
                SubjectEncoding = GetEncoding(template.HeaderEncoding),
                BodyEncoding = GetEncoding(template.BodyEncoding)
            };

            if (to.Length > 0)
            {
                mc.AddRecipient(to.First());

                if (to.Length > 1)
                {
                    foreach (var cc in to.Skip(1))
                    {
                        mc.AddCc(cc);
                    }
                }
            }

            IAttachment attachment = null;

            if (_timeTable.AttachmentType == AttachmentType.Fixed)
            {
                attachment = _attachments.Get(_timeTable.AttachmentName);
            }
            else if (_timeTable.AttachmentType == AttachmentType.Random)
            {
                var attachments = _attachments.All().ToArray();
                attachment = attachments[_random.Next(attachments.Length)];
            }

            if (attachment != null)
            {
                mc.AddAttachment(attachment.Name, attachment.Content);
            }

            if (_timeTable.SendEicarFile)
            {
                mc.AddAttachment("eicar.html", EICAR, Encoding.ASCII, MediaTypeNames.Text.Html);
            }

            if (EmlFolder != null)
            {
                if (!File.Exists(EmlFolder)) Directory.CreateDirectory(EmlFolder);

                var name = _timeTable.Name + " " + DateTime.Now.ToString("s");
                var safeName = Regex.Replace(name, "\\W+", "-");

                using (var file = File.OpenWrite(Path.Combine(EmlFolder, safeName + ".eml")))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(mc.ToString());
                }
            }

            return mc;
        }

        private string ReplaceTokens(string template, string name)
        {
            return template.Replace("[TEMPLATETITLE]", name)
                .Replace("[DATE]", DateTime.Today.ToString("D"))
                .Replace("[DATETIME]", DateTime.Now.ToString("F"))
                .Replace("[DATETIMEUTC]", DateTime.UtcNow.ToString("F"));
        }

        private Encoding GetEncoding(EncodingType type)
        {
            switch (type)
            {
                case EncodingType.ASCII:
                    return Encoding.ASCII;
                case EncodingType.UTF32:
                    return Encoding.UTF32;
                case EncodingType.UTF8:
                    return Encoding.UTF8;
                default:
                    throw new ArgumentException("Unknown encoding", "type");
            }
        }

        private TimeSpan GetWaitTime()
        {
            var wait = _timeTableType.GetNextMailTime() - DateTime.Now;

            if (wait < TimeSpan.Zero) wait = TimeSpan.Zero;

            return wait;
        }

        private string GetFrom()
        {
            if (_timeTable.StaticSender)
            {
                return _timeTable.SenderMailbox;
            }
            if (_timeTable.SenderGroupId != null)
            {
                var mailboxes = _localGroups.Get(_timeTable.SenderGroupId.Value).UserIds;

                var id = mailboxes[_random.Next(mailboxes.Length)];

                return _localUsers.Get(id).Mailbox;
            }
            var index = _random.Next(_localUsers.Total);
            return _localUsers.All().Skip(index).First().Mailbox;
        }

        private string[] GetRecipients()
        {
            if (_timeTable.StaticSender)
            {
                return new[] { _timeTable.RecipientMailbox };
            }

            var num = _random.Next(_timeTable.MinRecipients, _timeTable.MaxRecipients + 1);

            if (_timeTable.RecipientGroupId != null)
            {
                var mailboxes = _externalGroups.Get(_timeTable.RecipientGroupId.Value).UserIds;

                return mailboxes
                    .OrderBy(x => _random.Next())
                    .Take(num)
                    .Select(id => _externalUsers.Get(id).Mailbox)
                    .ToArray();
            }
            return _externalUsers
                .All()
                .OrderBy(x => _random.Next())
                .Take(num)
                .Select(u => u.Mailbox)
                .ToArray();
        }
    }
}