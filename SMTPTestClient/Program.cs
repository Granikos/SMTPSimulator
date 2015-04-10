using System.Net;
using System.Net.Mail;

namespace SMTPTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new SmtpClient("localhost", 1337))
            {
                var message = new MailMessage("tester@test.de", "tester2@test.de", "Subject", "Some body");

                client.Credentials = new NetworkCredential("username", "password");

                client.Send(message);
            }
        }
    }
}
