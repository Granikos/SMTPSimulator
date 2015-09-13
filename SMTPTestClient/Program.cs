using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using HydraCore;
using HydraService;
using HydraService.Models;

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

            var container = new CompositionContainer(new AssemblyCatalog(typeof (SMTPCore).Assembly));

            // var client = new SMTPClient("localhost", 1337)
            // var client = new SMTPClient("test.smtp.org")
            // var client = new SMTPClient("mailtrap.io", 2525)
            var client = SMTPClient.Create(container, new SendConnector(), "localhost", 25);

            client.Credentials = new NetworkCredential("357045ceeeb05fa28", "07db3fc0f21e46");

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
