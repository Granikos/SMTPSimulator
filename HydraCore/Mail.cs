using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace HydraCore
{
    public class Mail
    {
        public MailAddress From { get; set; }
        public MailAddressCollection Recipients { get; private set; }
        public Dictionary<string,string> Headers { get; private set; }
        public string Body { get; set; }

        public Mail(MailAddress from, IEnumerable<MailAddress> recipients, string mail)
        {
            From = from;
            Recipients = new MailAddressCollection();
            foreach (var recipient in recipients)
            {
                Recipients.Add(recipient);
            }
            Headers = new Dictionary<string, string>();
            Parse(mail);
        }

        private static readonly Regex WhitespaceRegex = new Regex("^\\s", RegexOptions.Compiled);

        private void Parse(string mail)
        {
            var lines = mail.Split(new [] {"\r\n"}, StringSplitOptions.None);

            if (!lines[0].Contains(":"))
            {
                Body = mail;
                return;
            }

            StringBuilder body = new StringBuilder();
            bool inBody = false;
            string lastHeader = null;

            foreach (var line in lines)
            {
                if (line.Equals(String.Empty))
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
                        var parts = line.Split(new [] {':'}, 2);

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
    }
}