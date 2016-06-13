using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;
using Granikos.NikosTwo.Service.PriorityQueue;
using Granikos.NikosTwo.SmtpClient;
using log4net;

namespace Granikos.NikosTwo.Service.TimeTables
{
    public class TimeTableGenerator
    {
        private const string EICAR = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
        public const double EicarProbability = 0.1;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TimeTableGenerator));
        private readonly ICollection<MailEvent> _events = new List<MailEvent>();
        private readonly object _lockObject = new object();
        private readonly DelayedQueue<SendableMail> _queue;
        private readonly Random _random = new Random();
        private readonly ITimeTable _timeTable;
        private readonly ITimeTableType _timeTableType;

        [Import]
        private IAttachmentProvider _attachments;

        [Import]
        private IExternalMailboxGroupProvider _externalGroups;

        [Import]
        private IExternalUserProvider _externalUsers;

        [Import]
        private ILocalMailboxGroupProvider _localGroups;

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private IMailTemplateProvider _mailTemplates;

        private Timer _reportTimer;
        private Timer _timer;

        [Import]
        private ITimeTableProvider _timeTables;

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

        private string EmlFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["EmlFolder"];
                return logFolder != null ? Path.Combine(folder, logFolder) : null;
            }
        }

        public bool IsRunning
        {
            get { return _timer != null; }
        }

        private ArggregatedEvent[] AggregateEvents(IEnumerable<MailEvent> events)
        {
            return events.GroupBy(e => new DateTime(e.Time.Year, e.Time.Month, e.Time.Day, e.Time.Hour, 0, 0))
                .Select(g => new ArggregatedEvent
                {
                    Time = g.Key,
                    Total = g.Count(),
                    Success = g.Count(r => r.Success),
                    Failure = g.Count(r => !r.Success),
                    Eicar = g.Count(r => r.Eicar)
                })
                .OrderBy(r => r.Time)
                .ToArray();
        }

        private string GenerateReportText(string text, string rowText, IEnumerable<ArggregatedEvent> events)
        {
            var result = ReplaceTokens(text, _timeTable.Name);

            result = ReplaceFormatted(result, "TOTAL", events.Sum(r => r.Total).ToString());
            result = ReplaceFormatted(result, "SUCCESSTOTAL", events.Sum(r => r.Success).ToString());
            result = ReplaceFormatted(result, "FAILURETOTAL", events.Sum(r => r.Failure).ToString());
            result = ReplaceFormatted(result, "EICARTOTAL", events.Sum(r => r.Eicar).ToString());
            result = result.Replace("[ACTIVESINCE]", _timeTable.ActiveSince != null ? _timeTable.ActiveSince.Value.ToString("F") : "---");
            result = result.Replace("[ACTIVEUNTIL]", _timeTable.ActiveUntil != null ? _timeTable.ActiveUntil.Value.ToString("F") : "---");

            return result.Replace("[ROWS]",
                string.Join("",
                    events.Select(r => string.Format(rowText, r.Time, r.Total, r.Success, r.Failure, r.Eicar))));
        }

        public MailContent GenerateReportMail(IEnumerable<MailEvent> mailEvents)
        {
            var text = ReportTemplateConfig.GetConfig().TextTemplate;
            var rowText = ReportTemplateConfig.GetConfig().RowTextTemplate;
            var html = ReportTemplateConfig.GetConfig().HtmlTemplate;
            var rowHtml = ReportTemplateConfig.GetConfig().RowHtmlTemplate;

            var events = AggregateEvents(mailEvents);

            var mailText = GenerateReportText(text, rowText, events);
            var mailHtml = GenerateReportText(html, rowHtml, events);

            var subject = ReplaceTokens(ReportTemplateConfig.GetConfig().SubjectTemplate, _timeTable.Name);

            var sender = ConfigurationManager.AppSettings["ReportSender"];

            var mail = new MailContent(subject, new MailAddress(sender, "Report"), mailHtml, mailText);

            mail.AddRecipient(_timeTable.ReportMailAddress, "Report");

            return mail;
        }

        private string ReplaceFormatted(string str, string name, string value)
        {
            str = str.Replace("[" + name + "]", value);

            for (var i = 2; i < 100; i++)
            {
                str = str.Replace("[" + name + ":" + i + "]", value.PadLeft(i));
            }

            return str;
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
                    _timer = new Timer(OnMailTimer, null, GetWaitTime(), TimeSpan.Zero);

                    DateTime? nextReportTime = null;
                    var reportFrequency = TimeSpan.Zero;

                    if (_timeTable.ReportType == ReportType.Daily)
                    {
                        nextReportTime = DateTime.Today.AddHours(_timeTable.ReportHour);

                        reportFrequency = TimeSpan.FromDays(1);
                    }
                    else if (_timeTable.ReportType == ReportType.Weekly)
                    {
                        var dayDiff = (7 + _timeTable.ReportDay - DateTime.Today.DayOfWeek) % 7;
                        nextReportTime = DateTime.Today.AddDays(dayDiff).AddHours(_timeTable.ReportHour);
                        reportFrequency = TimeSpan.FromDays(7);
                    }

                    if (nextReportTime != null)
                    {
                        if (nextReportTime < DateTime.Now)
                        {
                            nextReportTime += reportFrequency;
                        }

                        if (_timeTable.ProtocolLevel > ProtocolLevel.Off)
                        {
                            Logger.Info(string.Format("Timetable '{0}': next report is scheduled for {1}",
                                _timeTable.Name, nextReportTime));
                        }
                        _reportTimer = new Timer(OnReportTimer, null, nextReportTime.Value - DateTime.Now,
                            reportFrequency);
                    }
                }
            }
        }

        private void OnReportTimer(object state)
        {
            MailEvent[] events;
            lock (_events)
            {
                events = _events.ToArray();
                _events.Clear();
            }

            var mail = GenerateReportMail(events);

            if (_timeTable.ProtocolLevel > ProtocolLevel.Off)
            {
                Logger.InfoFormat("Timetable '{0}' sent a report.", _timeTable.Name);
            }

            _queue.Enqueue(new SendableMail(mail.From, mail.To, mail.ToString(), null), TimeSpan.Zero);

            if (EmlFolder != null)
            {
                if (!File.Exists(EmlFolder)) Directory.CreateDirectory(EmlFolder);

                var name = _timeTable.Name + " Report " + DateTime.Now.ToString("s");
                var safeName = Regex.Replace(name, "\\W+", "-");

                using (var file = File.OpenWrite(Path.Combine(EmlFolder, safeName + ".eml")))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(mail.ToString());
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

                if (_reportTimer != null)
                {
                    _reportTimer.Dispose();
                    _reportTimer = null;
                }
            }
        }

        private void OnMailTimer(object state)
        {
            try
            {
                var time = DateTime.Now;
                var sendEicar = _timeTable.SendEicarFile && new Random().NextDouble() <= EicarProbability;

                var from = GetFrom();
                var to = GetRecipients();
                var fromMail = new MailAddress(from);
                var toMail = to.Select(r => new MailAddress(r)).ToArray();
                var content = CreateContent(fromMail, toMail, sendEicar);

                var sendableMail = new SendableMail(fromMail, toMail, content.ToString(), null)
                {
                    ResultHandler = success =>
                    {
                        var evt = new MailEvent
                        {
                            Eicar = sendEicar,
                            Success = success,
                            Time = time
                        };

                        lock (_events)
                        {
                            _events.Add(evt);
                        }

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
                Logger.Error(
                    string.Format("Timetable '{0}': An exception occured while sending a mail.", _timeTable.Name), e);
            }
            finally
            {
                _timer.Change(GetWaitTime(), TimeSpan.Zero);
            }
        }

        private MailContent CreateContent(MailAddress from, MailAddress[] to, bool sendEicar)
        {
            var template = _mailTemplates.Get(_timeTable.MailTemplateId);

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

            if (sendEicar)
            {
                if (_timeTable.ProtocolLevel == ProtocolLevel.Verbose)
                {
                    Logger.InfoFormat("Timetable '{0}': next mail will be sent with an EICAR file.", _timeTable.Name);
                }

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
            if (_timeTable.SenderGroupId > 0)
            {
                var mailboxes = _localGroups.Get(_timeTable.SenderGroupId).UserIds;

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
                return new[] {_timeTable.RecipientMailbox};
            }

            var num = _random.Next(_timeTable.MinRecipients, _timeTable.MaxRecipients + 1);

            if (_timeTable.RecipientGroupId > 0)
            {
                var mailboxes = _externalGroups.Get(_timeTable.RecipientGroupId).UserIds;

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

        public class MailEvent
        {
            public DateTime Time { get; set; }
            public bool Success { get; set; }
            public bool Eicar { get; set; }
        }

        private class ArggregatedEvent
        {
            public DateTime Time { get; set; }
            public int Success { get; set; }
            public int Eicar { get; set; }
            public int Failure { get; set; }
            public int Total { get; set; }
        }
    }
}