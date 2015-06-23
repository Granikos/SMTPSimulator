using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using ARSoft.Tools.Net.Dns;
using HydraCore;
using HydraService.PriorityQueue;
using HydraService.Providers;

namespace HydraService
{
    class MessageSender
    {
        private readonly CompositionContainer _container;

        public MessageSender(CompositionContainer container)
        {
            _container = container;

            container.SatisfyImportsOnce(this);
        }

        readonly DelayedQueue<Mail> _mailQueue = new DelayedQueue<Mail>(1000);

        public void Enqueue(Mail mail, TimeSpan delay = default(TimeSpan))
        {
            _mailQueue.Enqueue(mail, delay);
        }

        private int _id = 1;

        [Import]
        private ISendConnectorProvider sendConnectors;

        public async void ProcessMail()
        {
            var mail = Dequeue();

            var badMailAdresses = new List<MailAddress>();

            foreach (var recipientGroup in mail.Recipients.GroupBy(r => r.Host))
            {
                var host = recipientGroup.Key;
                var connector = sendConnectors.DefaultConnector; // TODO
                    // .FirstOrDefault(s => s.Domains.Contains(host, StringComparer.InvariantCultureIgnoreCase));


                if (connector != null)
                {
                    string remoteHost;
                    int remotePort;

                    if (connector.UseSmarthost)
                    {
                        var response = await DnsClient.Default.ResolveAsync(recipientGroup.Key, RecordType.Mx);
                        var records = response.AnswerRecords.OfType<MxRecord>();
                        remoteHost = records.OrderBy(record => record.Preference).First().ExchangeDomainName;
                        remotePort = 25;
                    }
                    else
                    {
                        remoteHost = connector.RemoteAddress.ToString();
                        remotePort = connector.RemotePort;
                    }

                    var addresses = recipientGroup.Select(m => m.Address).ToArray();

                    var settings = new DefaultSendSettings(connector);

                    var client = SMTPClient.Create(_container, settings, remoteHost, remotePort);

                    var success = client.Connect();
                    success &= client.SendMail(mail.From.ToString(), addresses, mail.ToString());
                    client.Close();

                    if (!success)
                    {
                        // TODO
                    }
                }
            }

            if (badMailAdresses.Any())
            {
                Debug.WriteLine("Error with recipients: " + string.Join(", ", badMailAdresses.Select(m => m.ToString())));
                // Enqueue(CreateErrorMail(mail, badMailAdresses));
            }
        }

        private Mail Dequeue()
        {
            Mail mail = null;

            while (mail == null)
            {
                while (_mailQueue.Peek() == null) Thread.Sleep(100);

                mail = _mailQueue.Dequeue();
            }

            return mail;
        }

        private Mail CreateErrorMail(Mail original, IEnumerable<MailAddress> errorAddresses)
        {
            // TODO
            return original;
        }
    }
}
