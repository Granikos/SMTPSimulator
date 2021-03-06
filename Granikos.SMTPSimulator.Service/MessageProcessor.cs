﻿// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using ARSoft.Tools.Net.Dns;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.Core.Logging;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;
using Granikos.SMTPSimulator.Service.Providers;
using Granikos.SMTPSimulator.SmtpClient;

namespace Granikos.SMTPSimulator.Service
{
    public class NoMailHostFoundException : Exception
    {
        public NoMailHostFoundException(string host)
            : base(String.Format("No MX entry could be found for the domain '{0}'.", host))
        {
        }
    }

    internal class MessageProcessor
    {
        public delegate void MailErrorHandler(SendableMail mail, ConnectorInfo info, SMTPStatusCode? status, Exception e
            );

        private readonly CompositionContainer _container;

        [ImportMany]
        private IEnumerable<ISMTPLogger> _loggers;

        [Import]
        private ISendConnectorProvider sendConnectors;

        public MessageProcessor(CompositionContainer container)
        {
            _container = container;

            container.SatisfyImportsOnce(this);
        }

        private IEnumerable<ConnectorInfo> GroupByHost(SendableMail mail)
        {
            foreach (var recipientGroup in
                mail.Recipients
                    .GroupBy(r => r.Host))
            {
                var host = recipientGroup.Key;
                var connector = mail.Settings as ISendConnector
                    ?? sendConnectors.GetByDomain(host)
                    ?? sendConnectors.DefaultConnector;

                string remoteHost;
                int remotePort;

                if (!connector.UseSmarthost)
                {
                    var records = new DnsStubResolver().Resolve<MxRecord>(recipientGroup.Key);
                    var record = records.OrderBy(r => r.Preference).FirstOrDefault();

                    if (record == null)
                    {
                        TriggerMailError(mail, new ConnectorInfo
                        {
                            Connector = connector,
                            Host = recipientGroup.Key,
                            Port = 25,
                            Addresses = recipientGroup
                        }, null, new NoMailHostFoundException(recipientGroup.Key));
                        continue;
                    }
                    remoteHost = record.ExchangeDomainName.ToString();
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

        public event MailErrorHandler OnMailError;

        public bool ProcessMail(SendableMail mail)
        {
            var success = true;
            foreach (var connInfo in GroupByHost(mail))
            {
                var addresses = connInfo.Addresses.Select(a => a.Address).ToArray();
                var connector = connInfo.Connector;

                var client = SMTPClient.Create(_container, connector, connInfo.Host, connInfo.Port);

                if (!client.Connect())
                {
                    success = false;

                    TriggerMailError(mail, connInfo, client.LastStatus, client.LastException);
                    continue;
                }

                if (!client.SendMail(mail.From.Address, addresses, mail.ToString()))
                {
                    success = false;

                    TriggerMailError(mail, connInfo, client.LastStatus, client.LastException);
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

        protected virtual void TriggerMailError(SendableMail mail, ConnectorInfo info, SMTPStatusCode? status = null,
            Exception e = null)
        {
            var handler = OnMailError;
            if (handler != null) handler(mail, info, status, e);
        }

        public class ConnectorInfo
        {
            public IEnumerable<MailAddress> Addresses;
            public ISendConnector Connector;
            public string Host;
            public int Port;
        }
    }
}