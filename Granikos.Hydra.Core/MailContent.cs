using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Granikos.Hydra.Core
{
    public class Attachment
    {
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public Encoding Encoding { get; set; }

        public string Type { get; set; }
    }

    public class MailHeader
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public Encoding Encoding { get; set; }
    }

    public class MailContent
    {
        private readonly List<MailHeader> _additionalHeaders = new List<MailHeader>();
        private readonly List<Attachment> _attachments = new List<Attachment>();
        private readonly List<MailAddress> _to = new List<MailAddress>();
        private readonly List<MailAddress> _cc = new List<MailAddress>();
        private Encoding _bodyEncoding;
        private Encoding _headerEncoding;
        private Encoding _subjectEncoding;

        public MailContent(string subject, MailAddress @from, string html, string text)
        {
            Subject = subject;
            From = @from;
            Html = html;
            Text = text;

            var host = ConfigurationManager.AppSettings["MessageIdHost"];
            MessageId = string.Format("<{0}@{1}>", Guid.NewGuid().ToString("N"), host);
        }

        public string MessageId { get; set; }

        public string Subject { get; set; }

        public MailAddress From { get; set; }

        public IEnumerable<MailAddress> To
        {
            get { return _to; }
        }

        public IEnumerable<MailAddress> Cc
        {
            get { return _cc; }
        }

        public string Html { get; set; }

        public string Text { get; set; }

        public Encoding BodyEncoding
        {
            get { return _bodyEncoding ?? Encoding.Default; }
            set { _bodyEncoding = value; }
        }

        public Encoding SubjectEncoding
        {
            get { return _subjectEncoding ?? Encoding.Default; }
            set { _subjectEncoding = value; }
        }

        public Encoding HeaderEncoding
        {
            get { return _headerEncoding ?? Encoding.Default; }
            set { _headerEncoding = value; }
        }

        public IEnumerable<Attachment> Attachments
        {
            get { return _attachments; }
        }

        public IEnumerable<MailHeader> AdditionalHeaders
        {
            get { return _additionalHeaders; }
        }

        public void AddHeader(string name, string value, Encoding encoding = null)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            Contract.Requires<ArgumentNullException>(value != null, "value");

            _additionalHeaders.Add(new MailHeader { Name = name, Value = value, Encoding = encoding });
        }

        public void AddAttachment(string name, string content, Encoding encoding, string type = null)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            Contract.Requires<ArgumentNullException>(content != null, "content");
            Contract.Requires<ArgumentNullException>(encoding != null, "encoding");

            AddAttachment(name, encoding.GetBytes(content), encoding, type);
        }

        public void AddAttachment(string name, byte[] data, Encoding encoding = null, string type = null)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            Contract.Requires<ArgumentNullException>(data != null, "data");

            if (type == null)
            {
                type = MimeMapping.GetMimeMapping(name);
            }

            _attachments.Add(new Attachment { Name = name, Data = data, Encoding = encoding, Type = type });
        }

        public void AddRecipient(MailAddress recipient)
        {
            Contract.Requires<ArgumentNullException>(recipient != null, "recipient");

            _to.Add(recipient);
        }

        public void AddRecipient(string address, string displayName = null)
        {
            AddRecipient(new MailAddress(address, displayName));
        }

        public void AddCc(MailAddress recipient)
        {
            Contract.Requires<ArgumentNullException>(recipient != null, "recipient");

            _cc.Add(recipient);
        }

        public void AddCc(string address, string displayName = null)
        {
            AddCc(new MailAddress(address, displayName));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string boundary = Guid.NewGuid().ToString();
            var date = DateTime.Now.ToString("ddd, d MMM yyyy H:m:s zz00");

            var subject = !Equals(SubjectEncoding, Encoding.ASCII)
                ? EncodeQuotedPrintableHeader(Subject, SubjectEncoding)
                : Subject;

            AddHeader(sb, "From", FormatMailAddress(From, HeaderEncoding));
            if (To.Any()) AddHeader(sb, "To", String.Join(", ", To.Select(t => FormatMailAddress(t, HeaderEncoding))));
            if (Cc.Any()) AddHeader(sb, "Cc", String.Join(", ", Cc.Select(t => FormatMailAddress(t, HeaderEncoding))));
            AddHeader(sb, "Subject", subject);
            AddHeader(sb, "Content-Type", "multipart/mixed; boundary=" + boundary);
            AddHeader(sb, "MIME-Version", "1.0");
            AddHeader(sb, "Date", date);
            AddHeader(sb, "Message-ID", MessageId);

            foreach (var additionalHeader in AdditionalHeaders)
            {
                AddHeader(additionalHeader.Name, additionalHeader.Value);
            }

            AddLine(sb, "");

            AddLine(sb, "--" + boundary);

            string boundary2 = Guid.NewGuid().ToString();

            AddHeader(sb, "Content-Type", "multipart/alternative; boundary=" + boundary2);

            AddLine(sb, "");

            AddLine(sb, "--" + boundary2);

            AddHeader(sb, "Content-Type", "text/plain; charset=" + BodyEncoding.HeaderName);
            AddHeader(sb, "Content-Transfer-Encoding", "quoted-printable");
            AddHeader(sb, "Content-Disposition", "inline");

            AddLine(sb, "");

            AddLine(sb, encodeQuotedPrintable(Text, BodyEncoding));

            AddLine(sb, "--" + boundary2);

            AddHeader(sb, "Content-Type", "text/html; charset=" + BodyEncoding.HeaderName);
            AddHeader(sb, "Content-Transfer-Encoding", "quoted-printable");
            AddHeader(sb, "Content-Disposition", "inline");

            AddLine(sb, "");

            AddLine(sb, encodeQuotedPrintable(Html, BodyEncoding));

            AddLine(sb, "--" + boundary2 + "--");
            AddLine(sb, "");

            foreach (var attachment in Attachments)
            {
                AddLine(sb, "--" + boundary);

                if (attachment.Encoding != null)
                {
                    AddHeader(sb, "Content-Type", attachment.Type + "; charset=" + attachment.Encoding.HeaderName);
                }
                else
                {
                    AddHeader(sb, "Content-Type", attachment.Type);
                }
                AddHeader(sb, "Content-Transfer-Encoding", "base64");
                AddHeader(sb, "Content-Disposition", "attachment; filename=" + attachment.Name);

                AddLine(sb, "");

                AddLine(sb, Convert.ToBase64String(attachment.Data, Base64FormattingOptions.InsertLineBreaks));
            }

            AddLine(sb, "--" + boundary + "--");

            return sb.ToString();
        }
        private static bool IsAsciiString(string value)
        {
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        private static string FormatMailAddress(MailAddress address, Encoding encoding)
        {
            if (String.IsNullOrEmpty(address.DisplayName))
            {
                return address.Address;
            }

            return String.Format("{0} <{1}>", EncodeQuotedPrintableHeader(address.DisplayName, encoding),
                address.Address);
        }

        private string encodeQuotedPrintable(string input, Encoding encoding, params char[] additionalChars)
        {
            var sb = new StringBuilder();
            var bytes = encoding.GetBytes(input);

            for (int i = 0; i < bytes.Length; i++)
            {
                var ascii = bytes[i];

                var isPartOfLineBreak = (ascii == '\r' && i < bytes.Length - 1 && bytes[i + 1] == '\n')
                                        || (ascii == '\n' && i > 0 && bytes[i - 1] == '\r');

                if (!isPartOfLineBreak && (ascii < 32 || ascii == 61 || ascii > 126 || additionalChars.Contains((char)ascii)))
                {
                    sb.Append("=" + ascii.ToString("X2"));
                }
                else
                {
                    sb.Append((char) ascii);
                }
            }

            return SplitLine(sb.ToString(), "=\r\n");
        }

        private static string EncodeQuotedPrintableHeader(string input, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(input != null, "input");
            Contract.Requires<ArgumentNullException>(encoding != null, "encoding");

            if (input.Length == 0) return input;
            if (IsAsciiString(input)) return input;

            var sb = new StringBuilder();
            var bytes = encoding.GetBytes(input);

            var start = String.Format("=?{0}?Q?", encoding.HeaderName);
            var word = new StringBuilder(start, 75);

            foreach (var ascii in bytes)
            {
                string token;

                // Escape non-printable characters, non-ascii characters, as well as '=', '?', and '_'
                if (ascii <= 32 || ascii == 61 || ascii == 63 || ascii == 95 ||ascii > 126)
                {
                    token = "=" + ascii.ToString("X2");
                }
                else
                {
                    token = ((char) ascii).ToString();
                }

                // Split into 75 character encoded-words
                if (word.Length + token.Length > 73)
                {
                    word.Append("?= ");
                    sb.Append(word.ToString());
                    word.Clear();
                    word.Append(start);
                }

                word.Append(token);
            }

            sb.Append(word.ToString());
            sb.Append("?=");

            return sb.ToString();
        }

        private static void AddHeader(StringBuilder sb, string name, string value)
        {
            string header = String.Format("{0}: {1}", name, value);

            if (header.Length > 78)
            {
                header = SplitLine(header);
            }

            AddLine(sb, header);
        }

        private static string SplitLine(string input, string sep = "\r\n")
        {
            var parts = Regex.Split(input, "(\\s+)");

            var line = new StringBuilder(parts[0], 78 + sep.Length);
            var lines = new StringBuilder(input.Length + input.Length/20);

            for (int i = 2; i < parts.Length; i += 2)
            {
                if (parts[i - 1].Contains("\r\n"))
                {
                    lines.Append(line.ToString());
                    lines.Append(parts[i - 1]);
                    line.Clear();
                    line.Append(parts[i]);
                    continue;
                }

                if (line.Length + parts[i - 1].Length + parts[i].Length > 78)
                {
                    lines.Append(line.ToString());
                    line.Clear();
                    line.Append(sep);
                }

                line.Append(parts[i - 1]);
                line.Append(parts[i]);
            }

            if (line.Length > 0)
            {
                lines.Append(line.ToString());
            }

            return lines.ToString();
        }

        private static void AddLine(StringBuilder sb, string line, params object[] values)
        {
            sb.AppendFormat(line, values);
            sb.Append("\r\n");
        }
    }
}
