using Granikos.Hydra.Core;

namespace Granikos.Hydra.SmtpClient
{
    public class SendableMail : Mail
    {
        public SendableMail(Mail mail, ISendSettings settings)
            : base(mail.From, mail.Recipients, mail.Headers, mail.Body)
        {
            Settings = settings;
        }

        public int RetryCount { get; set; }
        public ISendSettings Settings { get; set; }
    }
}