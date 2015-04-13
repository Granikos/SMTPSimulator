using System.Net;
using System.Net.Mail;

namespace SMTPTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                return true;
            };

            using (var client = new SmtpClient("localhost", 1337))
            {
                client.EnableSsl = true;

                var message = new MailMessage("tester@test.de", "tester2@test.de", "Subject", "Some body");

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("username", "password");

                client.Send(message);
            }
        }
    }
}
