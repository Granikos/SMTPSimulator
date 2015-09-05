using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using HydraCore;
using HydraCore.Logging;
using HydraService.Models;
using HydraService.PriorityQueue;
using HydraService.Providers;

namespace HydraService
{
    internal class MessageProcessor
    {
        private readonly CompositionContainer _container;

        [ImportMany]
        private IEnumerable<ISMTPLogger> _loggers;

        [Import]
        private IDomainProvider domains;

        [Import]
        private ISendConnectorProvider sendConnectors;

        public MessageProcessor(CompositionContainer container)
        {
            _container = container;

            container.SatisfyImportsOnce(this);
        }

        public class ConnectorInfo
        {
            public SendConnector Connector;
            public string Host;
            public int Port;
            public IEnumerable<MailAddress> Addresses;
        }

        private IEnumerable<ConnectorInfo> GroupByHost(Mail mail)
        {
            foreach (var recipientGroup in
                mail.Recipients
                .GroupBy(r => r.Host))
            {
                var host = recipientGroup.Key;
                var domain = domains.GetByName(host);
                var connector = domain != null && domain.SendConnectorId != null
                    ? sendConnectors.Get((int)domain.SendConnectorId)
                    : sendConnectors.DefaultConnector;

                string remoteHost = null;
                int remotePort = 25;

                if (connector != null)
                {
                    if (connector.UseSmarthost)
                    {
                        var response = DnsClient.Default.Resolve(recipientGroup.Key, RecordType.Mx);
                        var records = response.AnswerRecords.OfType<MxRecord>();
                        remoteHost = records.OrderBy(record => record.Preference).First().ExchangeDomainName;
                        remotePort = 25;
                    }
                    else
                    {
                        remoteHost = connector.RemoteAddress.ToString();
                        remotePort = connector.RemotePort;
                    }
                }

                yield return new ConnectorInfo
                {
                    Connector = connector,
                    Host = remoteHost,
                    Port = remotePort,
                    Addresses = recipientGroup
                };
            }
        }

        public delegate void MailErrorHandler(Mail mail, ConnectorInfo info, SMTPStatusCode status);

        public event MailErrorHandler OnMailError;

        public void ProcessMail(Mail mail)
        {
            var badMailAdresses = new List<MailAddress>();

            foreach (var connInfo in GroupByHost(mail))
            {
                var addresses = connInfo.Addresses.Select(a => a.Address).ToArray();
                var connector = connInfo.Connector;

                var settings = new DefaultSendSettings(connector);

                var client = SMTPClient.Create(_container, settings, connInfo.Host, connInfo.Port);

                var status = client.Connect();

                if (status != null)
                {
                    TriggerMailError(mail, connInfo, status.Value);
                    break;
                }

                client.SendMail(mail.From.ToString(), addresses, mail.ToString());
                client.Close();

                // TODO
            }

            if (badMailAdresses.Any())
            {
                Debug.WriteLine("Error with recipients: " + string.Join(", ", badMailAdresses.Select(m => m.ToString())));
                // Enqueue(CreateErrorMail(mail, badMailAdresses));
            }
        }

        private void Log(LogEventType type, string data = null)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(null, null, null, null, LogPartType.Other, type, data);
            }
        }

        private Mail CreateErrorMail(Mail original, IEnumerable<MailAddress> errorAddresses)
        {
            // TODO
            return original;
        }

        protected virtual void TriggerMailError(Mail mail, ConnectorInfo info, SMTPStatusCode status)
        {
            var handler = OnMailError;
            if (handler != null) handler(mail, info, status);
        }
    }
}