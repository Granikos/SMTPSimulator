using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using ARSoft.Tools.Net.Dns;
using HydraCore;
using HydraCore.Logging;
using HydraService.Models;
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
                var connector = mail.Settings as SendConnector;
                if (connector == null)
                {
                    if (domain != null && domain.SendConnectorId != null)
                        connector = sendConnectors.Get((int)domain.SendConnectorId);
                    else connector = sendConnectors.DefaultConnector;
                }

                string remoteHost;
                int remotePort;

                if (!connector.UseSmarthost)
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

                yield return new ConnectorInfo
                {
                    Connector = connector,
                    Host = remoteHost,
                    Port = remotePort,
                    Addresses = recipientGroup
                };
            }
        }

        public delegate void MailErrorHandler(Mail mail, ConnectorInfo info, SMTPStatusCode? status, Exception e);

        public event MailErrorHandler OnMailError;

        public bool ProcessMail(Mail mail)
        {
            var success = true;
            foreach (var connInfo in GroupByHost(mail))
            {
                var addresses = connInfo.Addresses.Select(a => a.Address).ToArray();
                var connector = connInfo.Connector;

                var client = SMTPClient.Create(_container, connector, connInfo.Host, connInfo.Port);

                if (client.Connect())
                {
                    success = false;

                    TriggerMailError(mail, connInfo, client.LastStatus);
                    continue;
                }

                if (!client.SendMail(mail.From.ToString(), addresses, mail.ToString()))
                {
                    success = false;

                    TriggerMailError(mail, connInfo, client.LastStatus);
                }

                client.Close();

                // TODO
            }

            return success;
        }

        private void Log(LogEventType type, string data = null)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(null, null, null, null, LogPartType.Other, type, data);
            }
        }

        protected virtual void TriggerMailError(Mail mail, ConnectorInfo info, SMTPStatusCode? status = null, Exception e = null)
        {
            var handler = OnMailError;
            if (handler != null) handler(mail, info, status, e);
        }
    }
}