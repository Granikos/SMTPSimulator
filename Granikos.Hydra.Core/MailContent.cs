using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Mail;
using System.Text;

namespace Granikos.Hydra.Core
{
    public class Attachment
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public Encoding Encoding { get; set; }
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

        public MailContent(string subject, MailAddress @from, string html, string text)
        {
            Subject = subject;
            From = @from;
            Html = html;
            Text = text;
        }

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

        public Encoding ContentEncoding { get; set; }

        public Encoding SubjectEncoding { get; set; }

        public Encoding HeaderEncoding { get; set; }

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

        public void AddAttachment(string name, string content, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            Contract.Requires<ArgumentNullException>(content != null, "content");
            Contract.Requires<ArgumentNullException>(encoding != null, "encoding");

            _attachments.Add(new Attachment { Name = name, Content = content, Encoding = encoding });
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
    }
}
