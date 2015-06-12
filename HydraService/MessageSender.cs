using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks.Dataflow;
using ARSoft.Tools.Net.Dns;
using HydraCore;
using HydraService.Providers;
using Path = System.IO.Path;

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

        public string Diretory { get; set; }

        private readonly BufferBlock<Mail> _mails = new BufferBlock<Mail>();

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private IExternalUserProvider _externalUsers;

        public void Enqueue(Mail mail)
        {
            _mails.Post(mail);
        }

        private int _id = 1;

        public async void ProcessMail()
        {
            var mail = await _mails.ReceiveAsync();

            var badMailAdresses = new List<MailAddress>();

            foreach (var recipientGroup in mail.Recipients.GroupBy(r => r.Host))
            {
                List<MailAddress> externalMails = new List<MailAddress>();

                foreach (var recipient in recipientGroup)
                {
                    var local = _localUsers.GetByEmail(recipient.Address);

                    if (local != null)
                    {
                        using (var emlFile = File.Create(Path.Combine(Diretory, local.Id.ToString(), (_id++) + ".eml")))
                        using (var writer = new StreamWriter(emlFile))
                        {
                            writer.Write(mail.ToString());
                            writer.Flush();
                        }
                    }
                    else
                    {
                        var external = _externalUsers.GetByEmail(recipient.ToString());

                        if (external == null)
                        {
                            badMailAdresses.Add(recipient);
                        }
                        else
                        {
                            externalMails.Add(recipient);
                        }
                    }
                }

                if (externalMails.Any())
                {
                    var response = await DnsClient.Default.ResolveAsync(recipientGroup.Key, RecordType.Mx);
                    var records = response.AnswerRecords.OfType<MxRecord>();
                    var domain = records.OrderBy(record => record.Preference).First().ExchangeDomainName;

                    var client = SMTPClient.Create(_container, domain);

                    var success = client.Connect();
                    success &= client.SendMail(mail.From.ToString(), externalMails.Select(m => m.Address).ToArray(),
                        mail.ToString());
                    client.Close();

                    if (!success)
                    {
                        badMailAdresses.AddRange(externalMails);
                    }
                }
            }

            if (badMailAdresses.Any())
            {
                Debug.WriteLine("Error with recipients: " + string.Join(", ", badMailAdresses.Select(m => m.ToString())));
                // Enqueue(CreateErrorMail(mail, badMailAdresses));
            }
        }

        private Mail CreateErrorMail(Mail original, IEnumerable<MailAddress> errorAddresses)
        {
            // TODO
            return original;
        }
    }
}
