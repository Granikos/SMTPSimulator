using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Providers;
using log4net;

namespace Granikos.Hydra.Service.TimeTables
{
    public class TimeTableGenerator
    {
        private readonly TimeTable _timeTable;
        private readonly IMailQueueProvider _queue;
        private Thread _thread;
        private readonly object _lockObject = new object();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TimeTableGenerator));
        private readonly ITimeTableType _timeTableType;
        private readonly Random _random = new Random();
        private Timer _timer;

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private ILocalMailboxGroupProvider _localGroups;

        [Import]
        private IExternalUserProvider _externalUsers;

        [Import]
        private IExternalMailboxGroupProvider _externalGroups;

        public TimeTableGenerator(TimeTable timeTable, IMailQueueProvider queue, CompositionContainer container)
        {
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
                    Logger.Info(String.Format("Timetable '{0}' was started.", _timeTable.Name));
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
                    Logger.Info(String.Format("Timetable '{0}' was stopped.", _timeTable.Name));
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
                var mail = new MailMessage
                {
                    Content = _timeTable.MailContent,
                    Sender = from,
                    Recipients = to
                };

                Logger.InfoFormat("Timetable '{0}' enqueued a mail (From: {1}, To: {2}).", _timeTable.Name, from, String.Join(", ", to));

                _queue.Enqueue(mail);
            }
            catch (Exception e)
            {
                Logger.Error("An exception occured while sending a mail.", e);
            }
            finally
            {
                _timer.Change(GetWaitTime(), TimeSpan.Zero);
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
                var mailboxes = _localGroups.Get(_timeTable.SenderGroupId.Value).MailboxIds;

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
                var mailboxes = _externalGroups.Get(_timeTable.RecipientGroupId.Value).MailboxIds;

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
