using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net.Mail;
using ARSoft.Tools.Net.Dns;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;
using Granikos.NikosTwo.Service.PriorityQueue;
using Granikos.NikosTwo.SmtpClient;
using log4net;

namespace Granikos.NikosTwo.Service
{
    public delegate void MailErrorHandler(MailWithConnectorInfo info, SMTPStatusCode? status, Exception e);

    public class NoMailHostFoundException : Exception
    {
        public NoMailHostFoundException(string host)
            : base(String.Format("No MX entry could be found for the domain '{0}'.", host))
        {
        }
    }

    public class MailWithConnectorInfo
    {
        public IEnumerable<MailAddress> Addresses;
        public ISendConnector Connector;
        public string Host;
        public int Port;
        public SendableMail Mail;
    }

    public class MailDispatcher
    {
        private readonly ISendConnectorProvider _sendConnectors;
        private readonly CompositionContainer _container;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MailDispatcher));

        private readonly Dictionary<int, DelayedQueue<MailWithConnectorInfo>> _mailQueues = new Dictionary<int, DelayedQueue<MailWithConnectorInfo>>();

        private readonly IList<MessageSender> _senders = new List<MessageSender>();

        public event MailErrorHandler OnMailError;

        public MailDispatcher(ISendConnectorProvider sendConnectors, CompositionContainer container)
        {
            _sendConnectors = sendConnectors;
            _container = container;
            RefreshSendConnectors();
        }

        public void RefreshSendConnectors()
        {
            StopMessageProcessing();
            _mailQueues.Clear();
            _senders.Clear();

            foreach (var sendConnector in _sendConnectors.All())
            {
                var queue = new DelayedQueue<MailWithConnectorInfo>(1000);
                _mailQueues.Add(sendConnector.Id, queue);
                var sender = new MessageSender(_container, queue);
                sender.OnMailError += TriggerMailError;
                _senders.Add(sender);

                var connector = sendConnector;
                queue.OnQueueChanged += (_, args) =>
                {
                    PerformanceCounters.ForQueueLength(connector.Name).RawValue = queue.Count;
                };
            }

            StartMessageProcessing();
        }

        public void StopMessageProcessing()
        {
            if (_senders.Any())
            {
                Logger.Info("Stopping mail sending...");
                foreach (var sender in _senders)
                {
                    sender.Stop();
                }
                Logger.Info("Stopped mail sending.");
            }
        }

        public void StartMessageProcessing()
        {
            if (_senders.Any())
            {
                Logger.Info("Starting mail sending...");
                foreach (var sender in _senders)
                {
                    sender.Start();
                }
                Logger.Info("Started mail sending.");
            }
        }

        public void Enqueue(SendableMail mail, TimeSpan delay)
        {
            foreach (var info in GroupByHost(mail))
            {
                _mailQueues[info.Connector.Id].Enqueue(info, delay);
            } 
        }

        private IEnumerable<MailWithConnectorInfo> GroupByHost(SendableMail mail)
        {
            foreach (var recipientGroup in
                mail.Recipients
                    .GroupBy(r => r.Host))
            {
                var host = recipientGroup.Key;
                var connector = mail.Settings as ISendConnector
                    ?? _sendConnectors.GetByDomain(host)
                    ?? _sendConnectors.DefaultConnector;

                string remoteHost;
                int remotePort;

                if (!connector.UseSmarthost)
                {
                    var response = DnsClient.Default.Resolve(recipientGroup.Key, RecordType.Mx);
                    var records = response.AnswerRecords.OfType<MxRecord>();
                    var record = records.OrderBy(r => r.Preference).FirstOrDefault();

                    if (record == null)
                    {
                        TriggerMailError(new MailWithConnectorInfo
                        {
                            Connector = connector,
                            Host = recipientGroup.Key,
                            Port = 25,
                            Addresses = recipientGroup,
                            Mail = mail
                        }, null, new NoMailHostFoundException(recipientGroup.Key));
                        continue;
                    }
                    remoteHost = record.ExchangeDomainName;
                    remotePort = 25;
                }
                else
                {
                    remoteHost = connector.RemoteAddress.ToString();
                    remotePort = connector.RemotePort;
                }

                yield return new MailWithConnectorInfo
                {
                    Connector = connector,
                    Host = remoteHost,
                    Port = remotePort,
                    Addresses = recipientGroup,
                    Mail = mail
                };
            }
        }

        protected virtual void TriggerMailError(MailWithConnectorInfo info, SMTPStatusCode? status = null,
            Exception e = null)
        {
            var connector = info.Connector;
            if (status == SMTPStatusCode.NotAvailiable)
            {
                if (info.Mail.RetryCount < connector.RetryCount)
                {
                    Logger.InfoFormat("Remote host was not availiable, retrying to send mail in {0} (try {1}/{2})",
                        connector.RetryTime, info.Mail.RetryCount + 1, connector.RetryCount + 1);
                    info.Mail.RetryCount++;
                    var queue = _mailQueues[connector.Id];
                    lock (queue)
                    {
                        queue.Enqueue(info, connector.RetryTime);
                    }
                }
                else
                {
                    Logger.InfoFormat("Remote host was not availiable, but retry count exceeded (try {0}/{0}).",
                        connector.RetryCount + 1);
                }
            }

            if (e != null)
            {
                Logger.Error("An error occured while sending a mail:", e);
            }

            var handler = OnMailError;
            if (handler != null) handler(info, status, e);
        }
    }
}
