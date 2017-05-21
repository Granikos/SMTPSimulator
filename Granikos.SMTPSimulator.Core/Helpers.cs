using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Granikos.SMTPSimulator.Core
{
    public static class Helpers
    {
        public static string ToSMTPString(this string str)
        {
            if (str == null) throw new ArgumentNullException();
            if (RegularExpressions.DotStringRegex.IsMatch(str)) return str;

            var sb = new StringBuilder(str.Length + 2);

            sb.Append('"');

            foreach (var chr in str)
            {
                if (chr == '"' || chr == '\\')
                {
                    sb.Append('\\');
                }

                sb.Append(chr);
            }
            sb.Append('"');

            return sb.ToString();
        }

        // No validation done!
        public static string FromSMTPString(this string str)
        {
            if (str == null) throw new ArgumentNullException();
            if (!str.StartsWith("\"")) return str;

            var sb = new StringBuilder(str.Length - 2);

            var escaped = false;
            foreach (var chr in str.Substring(1, str.Length - 2))
            {
                if (!escaped && chr == '\\')
                {
                    escaped = true;
                }
                else
                {
                    sb.Append(chr);
                    escaped = false;
                }
            }

            return sb.ToString();
        }

        public static MailAddressCollection ToMailAddressCollection(this IEnumerable<MailAddress> addresses)
        {
            var collection = new MailAddressCollection();
            foreach (var address in addresses)
            {
                collection.Add(address);
            }

            return collection;
        }
    }
}