using System;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using HydraCore;
using HydraService.PriorityQueue;

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
                if (mail.RetryCount > connector.RetryCount)
                {
                    mail.RetryCount++;
                    lock (_mailQueue)
                    {
                        _mailQueue.Enqueue(mail, connector.RetryTime);
                    }
                }
                else
                {
                    // TODO
                    /*
                    Log(LogEventType.Other,
                        string.Format("Retry count '{0}' exceeded while trying to deliver via '{1}'",
                            connector.RetryCount, connector.Name));

                    // Error mail?
                    */
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