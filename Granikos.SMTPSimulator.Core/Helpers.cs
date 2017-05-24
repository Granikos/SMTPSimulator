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