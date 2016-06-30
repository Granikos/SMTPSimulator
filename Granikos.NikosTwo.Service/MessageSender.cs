using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Service.PriorityQueue;
using Granikos.NikosTwo.SmtpClient;
using log4net;

namespace Granikos.NikosTwo.Service
{
    internal class MessageSender
    {
        public delegate void ThreadFinishedEventHandler();

        private static readonly ILog Logger = LogManager.GetLogger(typeof (MessageSender));
        private readonly ManualResetEvent _askStop;
        private readonly ManualResetEvent _informStopped;
        private readonly object _lockObject = new object();
        private readonly CompositionContainer _container;
        private readonly DelayedQueue<MailWithConnectorInfo> _mailQueue;
        private Thread _thread;
        protected int TickDefaultMilliseconds = 1000;

        public event MailErrorHandler OnMailError;

        public MessageSender(CompositionContainer container, DelayedQueue<MailWithConnectorInfo> queue)
        {
            _askStop = new ManualResetEvent(false);
            _informStopped = new ManualResetEvent(false);

            _container = container;
            _mailQueue = queue;
        }

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

        public event ThreadFinishedEventHandler ThreadFinishedEvent;

        private void RaiseFinishedEvent()
        {
            if (ThreadFinishedEvent != null)
            {
                ThreadFinishedEvent();
            }
        }

        public virtual void Start()
        {
            lock (_lockObject)
            {
                if (_thread != null)
                {
                    // already running state
                    return;
                }
                _askStop.Reset();
                _informStopped.Reset();
                _thread = new Thread(WorkerCallback) {IsBackground = true};
                _thread.Start();
            }
        }

        protected virtual void WorkerCallback()
        {
            while (true)
            {
                try
                {
                    MailWithConnectorInfo mail = null;
                    lock (_mailQueue)
                    {
                        if (_mailQueue.Peek() != null)
                        {
                            mail = _mailQueue.Dequeue();
                        }
                    }

                    if (mail != null)
                    {
                        var success = ProcessMail(mail);

                        if (mail.Mail.ResultHandler != null)
                        {
                            mail.Mail.ResultHandler(success);
                        }

                        if (success)
                        {
                            PerformanceCounters.TriggerSent();
                        }
                    }

                    Thread.Sleep(TickDefaultMilliseconds);

                    if (_askStop.WaitOne(0, true))
                    {
                        _informStopped.Set();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("An exception occured while sending a mail.", e);
                }
            }

            RaiseFinishedEvent();
        }

        public bool ProcessMail(MailWithConnectorInfo mail)
        {
            var addresses = mail.Addresses.Select(a => a.Address).ToArray();
            var connector = mail.Connector;

            var client = SMTPClient.Create(_container, connector, mail.Host, mail.Port);

                if (!client.Connect())
                {

                    TriggerMailError(mail, client.LastStatus, client.LastException);

                    return false;
                }

                if (!client.SendMail(mail.Mail.From.Address, addresses, mail.ToString()))
                {
                    TriggerMailError(mail, client.LastStatus, client.LastException);

                    return false;
                }

                client.Close();

            return true;
        }

        public bool HasStopped()
        {
            return _informStopped.WaitOne(1, true);
        }

        public virtual void Stop()
        {
            lock (_lockObject)
            {
                // todo fix faulted state exception
                if (_thread != null && _thread.IsAlive) // thread is active
                {
                    _askStop.Set();
                    while (_thread.IsAlive)
                    {
                        if (_informStopped.WaitOne(100, true))
                        {
                            break;
                        }
                    }
                    _thread = null;
                }
            }
        }

        protected virtual void TriggerMailError(MailWithConnectorInfo info, SMTPStatusCode? status = null,
            Exception e = null)
        {
            var handler = OnMailError;
            if (handler != null) handler(info, status, e);
        }
    }
}