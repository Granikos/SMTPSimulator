using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Granikos.SMTPSimulator.Core
{
    public sealed class MailPath
    {
        public static readonly MailPath Empty = new MailPath("");
        public static readonly MailPath Postmaster = new MailPath("postmaster");
        public readonly string[] AtDomains;
        public readonly string Domain;
        public readonly string LocalPart;

        public MailPath(string localPart, string domain, params string[] atDomains)
        {
            if (localPart == null) throw new ArgumentNullException();
            if (domain == null) throw new ArgumentNullException();
            if (atDomains == null) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(localPart)) throw new ArgumentException();
            if (string.IsNullOrEmpty(domain)) throw new ArgumentException();

            LocalPart = localPart;
            Domain = domain;
            AtDomains = atDomains;
        }

        private MailPath(string localPart)
        {
            LocalPart = localPart;
            Domain = "";
            AtDomains = new string[0];
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Domain)) return string.Format("<{0}>", LocalPart);
            if (!AtDomains.Any()) return string.Format("<{0}@{1}>", LocalPart.ToSMTPString(), Domain);

            var ad = string.Join(",", AtDomains.Select(d => "@" + d));

            return string.Format("<{2}:{0}@{1}>", LocalPart.ToSMTPString(), Domain, ad);
        }

        public bool Equals(MailPath other)
        {
            if (ReferenceEquals(null, other)) return false;

            return string.Equals(LocalPart, other.LocalPart, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(Domain, other.Domain, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MailPath && Equals((MailPath) obj);
        }

        public MailAddress ToMailAdress()
        {
            return string.IsNullOrEmpty(Domain)
                ? null
                : new MailAddress(LocalPart + "@" + Domain);
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LocalPart.GetHashCode();
                hashCode = (hashCode*397) ^ Domain.GetHashCode();
                return hashCode;
            }
        }

        public static MailPath FromString(string str)
        {
            if (str == null) throw new ArgumentNullException();

            var match = RegularExpressions.PathRegex.Match(str);

            if (!match.Success) throw new ArgumentException("The given string is not a valid SMTP path.");

            return FromMatch(match);
        }

        public static MailPath FromMatch(Match match)
        {
            if (!(match.Success)) throw new ArgumentException();

            var atDomains = match.Groups["AtDomains"].Value.Split(',')
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Substring(1))
                .ToArray();
            var localPart = match.Groups["LocalPart"].Value.FromSMTPString();
            var domain = match.Groups["Domain"].Value;

            return new MailPath(localPart, domain, atDomains);
        }
    }
}