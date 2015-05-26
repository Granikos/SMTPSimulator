using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace HydraService
{
    public static class MailExtensions
    {
        public static string ToRaw(this MailMessage message)
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
                    message,
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

        public static void WriteToStream(this MailMessage message, Stream stream)
        {
            Assembly assembly = typeof(SmtpClient).Assembly;
            Type _mailWriterType =
                assembly.GetType("System.Net.Mail.MailWriter");

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
                message,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { _mailWriter, true, false },
                null);

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
        }
    }
}