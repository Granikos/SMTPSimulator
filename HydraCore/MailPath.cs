using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace HydraCore
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
            Contract.Requires<ArgumentNullException>(localPart != null);
            Contract.Requires<ArgumentNullException>(domain != null);
            Contract.Requires<ArgumentNullException>(atDomains != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(localPart));
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(domain));

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
            if (String.IsNullOrEmpty(Domain)) return String.Format("<{0}>", LocalPart);
            if (!AtDomains.Any()) return String.Format("<{0}@{1}>", LocalPart.ToSMTPString(), Domain);

            var ad = String.Join(",", AtDomains.Select(d => "@" + d));

            return String.Format("<{2}:{0}@{1}>", LocalPart.ToSMTPString(), Domain, ad);
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
            return String.IsNullOrEmpty(Domain)
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
            Contract.Requires<ArgumentNullException>(str != null);

            var match = RegularExpressions.PathRegex.Match(str);

            if (!match.Success) throw new ArgumentException("The given string is not a valid SMTP path.");

            return FromMatch(match);
        }

        public static MailPath FromMatch(Match match)
        {
            Contract.Requires<ArgumentException>(match.Success);

            var atDomains = match.Groups["AtDomains"].Value.Split(',')
                .Where(d => !String.IsNullOrWhiteSpace(d))
                .Select(d => d.Substring(1))
                .ToArray();
            var localPart = match.Groups["LocalPart"].Value.FromSMTPString();
            var domain = match.Groups["Domain"].Value;

            return new MailPath(localPart, domain, atDomains);
        }
    }
}