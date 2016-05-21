using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Reflection;
using System.Text;
using Granikos.NikosTwo.Core;
// using Granikos.NikosTwo.ImapClient;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Retention;
using Granikos.NikosTwo.SmtpClient;
using Granikos.NikosTwo.SmtpServer;
using S22.Imap;

// using System.Net.Mail;

namespace SMTPTestClient
{
    class TL : TraceListener
    {
        readonly StringBuilder _sb = new StringBuilder();

        public override void Write(string message)
        {

        }

        public override void WriteLine(string message)
        {
            _sb.AppendLine(message.Replace("S -> ", "> ").Replace("C -> ", "< "));
        }

        public string GetLog()
        {
            return _sb.ToString();
        }
    }

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
                Password = "07db3fc0f21e46",
                TLSSettings = new TLSSettings
                {
                    Mode = TLSMode.FullTunnel
                }
            };

            /*
            var client = ImapClient.Create(sc, "imap.gmail.com", 993);
            client.Stream.OnError += e => { throw e; };

            if (client.Connect())
            {
                Console.WriteLine("Connected");

                client.Stream.Write("CAPABILITY").OnOkay(resp =>
                {
                    Console.WriteLine("Okay, {0}", resp.Response);
                });
            }
             * */
            var field = typeof(ImapClient).GetField("ts", BindingFlags.NonPublic | BindingFlags.Static);

            var ts = (TraceSource)field.GetValue(null);
            var tl = new TL();
            ts.Listeners.Add(tl);
            ts.Switch.Level = SourceLevels.All;

            using (var client = new ImapClient("imap.gmail.com", 993, true))
            {
                foreach (var cap in client.Capabilities())
                {
                    Console.WriteLine(cap);
                }
            }

            Console.WriteLine();
            Console.WriteLine("IMAP Log:");
            Console.WriteLine(tl.GetLog());

            Console.ReadLine();
        }
    }
}
