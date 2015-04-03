using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace HydraCore
{
    public static class Helpers
    {
        public static string ToSMTPString(this string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            if (RegularExpressions.DotStringRegex.IsMatch(str)) return str;

            StringBuilder sb = new StringBuilder(str.Length + 2);

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
            Contract.Requires<ArgumentNullException>(str != null);
            if (!str.StartsWith("\"")) return str;

            StringBuilder sb = new StringBuilder(str.Length - 2);

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
    }
}
