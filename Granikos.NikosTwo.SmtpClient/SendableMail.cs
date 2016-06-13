using System.Collections.Generic;
using System.Net.Mail;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpClient
{
    public delegate void MailResultHandler(bool success);

    public class SendableMail : Mail
    {
        public SendableMail(MailAddress from, IEnumerable<MailAddress> recipients, string mail, ISendSettings settings)
            : base(from, recipients, mail)
        {
            Settings = settings;
        }

        public SendableMail(Mail mail, ISendSettings settings)
            : base(mail.From, mail.Recipients, mail.Headers, mail.Body)
        {
            Settings = settings;
        }

        public int RetryCount { get; set; }
        public ISendSettings Settings { get; set; }

        public MailResultHandler ResultHandler { get; set; }
    }
}