// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Granikos.SMTPSimulator.SmtpClient;

namespace Granikos.SMTPSimulator.Service
{
    public static class MailExtensions
    {
        public static string ToRaw(this MailMessage message)
        {
            var assembly = typeof (SMTPClient).Assembly;
            var _mailWriterType =
                assembly.GetType("System.Net.Mail.MailWriter");

            using (Stream stream = new MemoryStream(500))
            {
                // Get reflection info for MailWriter contructor
                var _mailWriterContructor =
                    _mailWriterType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] {typeof (Stream)},
                        null);

                // Construct MailWriter object with our FileStream
                var _mailWriter =
                    _mailWriterContructor.Invoke(new object[] {stream});

                // Get reflection info for Send() method on MailMessage
                var _sendMethod =
                    typeof (MailMessage).GetMethod(
                        "Send",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call method passing in MailWriter
                _sendMethod.Invoke(
                    message,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] {_mailWriter, true, false},
                    null);

                string str;

                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.ASCII, false, 200, true))
                {
                    str = reader.ReadToEnd();
                }

                // Finally get reflection info for Close() method on our MailWriter
                var _closeMethod =
                    _mailWriter.GetType().GetMethod(
                        "Close",
                        BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                _closeMethod.Invoke(
                    _mailWriter,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] {},
                    null);

                return str;
            }
        }

        public static void WriteToStream(this MailMessage message, Stream stream)
        {
            var assembly = typeof (System.Net.Mail.SmtpClient).Assembly;
            var _mailWriterType =
                assembly.GetType("System.Net.Mail.MailWriter");

            // Get reflection info for MailWriter contructor
            var _mailWriterContructor =
                _mailWriterType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] {typeof (Stream)},
                    null);

            // Construct MailWriter object with our FileStream
            var _mailWriter =
                _mailWriterContructor.Invoke(new object[] {stream});

            // Get reflection info for Send() method on MailMessage
            var _sendMethod =
                typeof (MailMessage).GetMethod(
                    "Send",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            // Call method passing in MailWriter
            _sendMethod.Invoke(
                message,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] {_mailWriter, true, false},
                null);

            // Finally get reflection info for Close() method on our MailWriter
            var _closeMethod =
                _mailWriter.GetType().GetMethod(
                    "Close",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            // Call close method
            _closeMethod.Invoke(
                _mailWriter,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] {},
                null);
        }
    }
}