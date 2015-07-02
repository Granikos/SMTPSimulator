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
        public static string MailToString(MailMessage Message)
        {
            Assembly assembly = typeof(SmtpClient).Assembly;
            Type _mailWriterType =
              assembly.GetType("System.Net.Mail.MailWriter");

            using (Stream stream = new MemoryStream(500))
            {
                // Get reflection info for MailWriter contructor
                ConstructorInfo _mailWriterContructor =
                    _mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(Stream) },
                        null);

                // Construct MailWriter object with our FileStream
                object _mailWriter =
                  _mailWriterContructor.Invoke(new object[] { stream });

                // Get reflection info for Send() method on MailMessage
                MethodInfo _sendMethod =
                    typeof(MailMessage).GetMethod(
                        "Send",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter
                _sendMethod.Invoke(
                    Message,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { _mailWriter, true, false },
                    null);

                string str;

                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.ASCII, false, 200, true))
                {
                    str = reader.ReadToEnd();

                }

                // Finally get reflection info for Close() method on our MailWriter
                MethodInfo _closeMethod =
                    _mailWriter.GetType().GetMethod(
                        "Close",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                _closeMethod.Invoke(
                    _mailWriter,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { },
                    null);

                return str;
        }
        }

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                return true;
            };

            var container = new CompositionContainer(new AssemblyCatalog(typeof (SMTPCore).Assembly));

            var settings = new DefaultSendSettings(new SendConnector());
            // var client = new SMTPClient("localhost", 1337)
            // var client = new SMTPClient("test.smtp.org")
            // var client = new SMTPClient("mailtrap.io", 2525)
            var client = SMTPClient.Create(container, settings, "localhost", 25);

            client.Credentials = new NetworkCredential("357045ceeeb05fa28", "07db3fc0f21e46");

            var mailMessage = new MailMessage("manuel.krebber@granikos.eu", "manuel.krebber@outlook.com", "Test", "Fubar.");

            var str = MailToString(mailMessage);


            client.Connect();
            client.SendMail("manuel.krebber@granikos.eu", new[] { "manuel.krebber@outlook.com" }, str);
            client.Close();

            /*
            using (var client = new SmtpClient("localhost", 1337))
            {
                client.EnableSsl = true;

                var message = new MailMessage("tester@test.de", "tester2@test.de", "Subject", "Some body");

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("username", "password");

                client.Send(message);
            }
             * */
        }
    }
}
