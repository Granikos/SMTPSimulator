using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Net.Mail;
using Granikos.Hydra.Core;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.SmtpClient;
using Granikos.Hydra.SmtpServer;

// using System.Net.Mail;

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

            var container = new CompositionContainer(new AssemblyCatalog(typeof (SMTPServer).Assembly));

            // var client = new SMTPClient("localhost", 1337)
            // var client = new SMTPClient("test.smtp.org")
            // var client = new SMTPClient("mailtrap.io", 2525)
            var sc = new SendConnector
            {
                Username = "357045ceeeb05fa28",
                Password = "07db3fc0f21e46"
            };
            var client = SMTPClient.Create(container, sc, "localhost", 25);

            var from = new MailAddress("manuel.krebber@granikos.eu");
            var to = new MailAddress("manuel.krebber@outlook.com");

            string mail = "Test";

            var mailMessage = new Mail(from, new[] { to }, mail);

            if (client.Connect())
            {
                client.SendMail("manuel.krebber@granikos.eu", new[] {"manuel.krebber@outlook.com"}, mail);
                client.Close();
            }

            /*
            using (var client = new SmtpClient("localhost", 1337))
            {
                client.TLSFullTunnel = true;

                var message = new MailMessage("tester@test.de", "tester2@test.de", "Subject", "Some body");

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("username", "password");

                client.Send(message);
            }
             * */
        }
    }
}
