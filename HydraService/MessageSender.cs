using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Mail;
using HydraCore;
using HydraService.Providers;
using Path = System.IO.Path;

namespace HydraService
{
    class MessageSender
    {
        public string Diretory { get; set; }

        private readonly Queue<Mail> _mails = new Queue<Mail>();

        [Import]
        private ILocalUserProvider _localUsers;

        [Import]
        private IExternalUserProvider _externalUsers;

        public void Enqueue(Mail mail)
        {
            _mails.Enqueue(mail);
        }

        private int _id = 1;

        public void ProcessMail()
        {
            if (_mails.Count > 0)
            {
                var mail = _mails.Dequeue();
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
                        var client = new SMTPClient(recipientGroup.Key);

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
                    Enqueue(CreateErrorMail(mail, badMailAdresses));
                }
            }
        }

        private Mail CreateErrorMail(Mail original, IEnumerable<MailAddress> errorAddresses)
        {
            // TODO
            return original;
        }
    }
}
