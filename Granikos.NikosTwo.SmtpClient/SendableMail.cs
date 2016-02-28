using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpClient
{
    public delegate void MailResultHandler(bool success);

    public class SendableMail : Mail
    {
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