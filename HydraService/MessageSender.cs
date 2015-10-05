using System;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using HydraCore;
using HydraService.PriorityQueue;
using log4net;

namespace HydraService
{
    internal class MessageSender
    {
        private Thread _thread;
        private readonly ManualResetEvent _askStop;
        private readonly ManualResetEvent _informStopped;

        public delegate void ThreadFinishedEventHandler();
        public event ThreadFinishedEventHandler ThreadFinishedEvent;

        private readonly object _lockObject = new object();

        private readonly MessageProcessor _processor;
        private readonly DelayedQueue<Mail> _mailQueue;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageSender));

        private void RaiseFinishedEvent()
        {
            if (ThreadFinishedEvent != null)
            {
                ThreadFinishedEvent();
            }
        }

        public MessageSender(CompositionContainer container, DelayedQueue<Mail> queue)
        {
            _askStop = new ManualResetEvent(false);
            _informStopped = new ManualResetEvent(false);

            _processor = new MessageProcessor(container);
            _mailQueue = queue;

            _processor.OnMailError += HandleMailError;
        }

        public void HandleMailError(Mail mail, MessageProcessor.ConnectorInfo info, SMTPStatusCode? status, Exception e)
        {
            var connector = info.Connector;
            if (status == SMTPStatusCode.NotAvailiable)
            {
                if (mail.RetryCount < connector.RetryCount)
                {
                    Logger.InfoFormat("Remote host was not availiable, retrying to send mail in {0} (try {1}/{2})", connector.RetryTime, mail.RetryCount + 1, connector.RetryCount + 1);
                    mail.RetryCount++;
                    lock (_mailQueue)
                    {
                        _mailQueue.Enqueue(mail, connector.RetryTime);
                    }
                }
                else
                {
                    Logger.InfoFormat("Remote host was not availiable, but retry count exceeded (try {0}/{0}).", connector.RetryCount + 1);
                }
            }

            // TODO
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
                _thread = new Thread(WorkerCallback) { IsBackground = true };
                _thread.Start();
            }
        }

        protected int TickDefaultMilliseconds = 1000;

        protected virtual void WorkerCallback()
        {
            while (true)
            {
                try
                {
                    Mail mail = null;
                    lock (_mailQueue)
                    {
                        if (_mailQueue.Peek() != null)
                        {
                            mail = _mailQueue.Dequeue();
                        }
                    }

                    if (mail != null)
                    {
                        _processor.ProcessMail(mail);
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

        public bool HasStopped()
        {
            return _informStopped.WaitOne(1, true);
        }

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
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

    }
}