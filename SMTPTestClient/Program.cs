using System;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Net.Mail;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Retention;
using Granikos.NikosTwo.SmtpClient;
using Granikos.NikosTwo.SmtpServer;

// using System.Net.Mail;

namespace SMTPTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var basedir =
                @"C:\Users\Manuel\Documents\Visual Studio 2013\Projects\NikosTwo\Granikos.NikosTwo.Service\bin\Debug\Logs\SystemLogs\Service";

            var manager = new RetentionWorker(basedir);

            manager.Config.MinTime = TimeSpan.FromDays(5);
            // manager.Config.MaxTime = TimeSpan.FromDays(20);
            // manager.Config.MaxFiles = 10;

            manager.Config.MaxSize = 1024*1024;

            manager.Run();

            Console.ReadLine();

            /*
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
