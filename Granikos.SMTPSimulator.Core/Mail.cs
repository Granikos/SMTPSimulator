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
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Granikos.SMTPSimulator.Core
{
    public class Mail
    {
        private static readonly Regex WhitespaceRegex = new Regex("^\\s", RegexOptions.Compiled);

        private readonly string rawMail;

        public Mail(MailAddress from, IEnumerable<MailAddress> recipients, string mail)
        {
            From = from;
            Recipients = recipients.ToMailAddressCollection();
            Headers = new Dictionary<string, string>();

            rawMail = mail;
            Parse(mail);
        }

        protected Mail(MailAddress from, IEnumerable<MailAddress> recipients, Dictionary<string, string> headers, string content)
        {
            From = from;
            Recipients = recipients.ToMailAddressCollection();
            Headers = headers;
            Body = content;
        }

        public MailAddress From { get; set; }
        public MailAddressCollection Recipients { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; set; }

        private void Parse(string mail)
        {
            var lines = mail.Split(new[] {"\r\n"}, StringSplitOptions.None);

            if (!lines[0].Contains(":"))
            {
                Body = mail;
                return;
            }

            var body = new StringBuilder();
            var inBody = false;
            string lastHeader = null;

            foreach (var line in lines)
            {
                if (line.Equals(string.Empty))
                {
                    inBody = true;
                }
                else if (inBody)
                {
                    body.AppendLine(line);
                }
                else
                {
                    if (WhitespaceRegex.Match(line).Success)
                    {
                        if (lastHeader != null) Headers[lastHeader] += line;
                    }
                    else
                    {
                        var parts = line.Split(new[] {':'}, 2);

                        lastHeader = parts[0];

                        if (parts.Length == 2)
                        {
                            Headers[lastHeader] = parts[1];
                        }
                    }
                }
            }

            Body = body.ToString();
        }

        public override string ToString()
        {
            // TODO: Quick hack, fixme!
            if (rawMail != null) return rawMail;

            var sb = new StringBuilder();

            foreach (var header in Headers)
            {
                sb.Append(header.Key);
                sb.Append(": ");
                sb.Append(header.Value);
                sb.Append("\r\n");
            }

            sb.Append("\r\n");
            sb.Append(Body);

            return sb.ToString();
        }
    }
}