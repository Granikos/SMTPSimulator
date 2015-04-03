using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace HydraCore
{
    public sealed class Path
    {
        public Path(string localPart, string domain, params string[] atDomains)
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

        private Path()
        {
            LocalPart = "";
            Domain = "";
            AtDomains = new string[0];
        }

        public static readonly Path Empty = new Path();

        public readonly string LocalPart;
        public readonly string Domain;
        public readonly string[] AtDomains;

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Domain)) return "<>";
            if (!AtDomains.Any()) return String.Format("<{0}@{1}>", LocalPart.ToSMTPString(), Domain);

            var ad = String.Join(",", AtDomains.Select(d => "@" + d));

            return String.Format("<{2}:{0}@{1}>", LocalPart.ToSMTPString(), Domain, ad);
        }

        public bool Equals(Path other)
        {
            if (ReferenceEquals(null, other)) return false;

            return string.Equals(LocalPart, other.LocalPart, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(Domain, other.Domain, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Path && Equals((Path) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LocalPart.GetHashCode();
                hashCode = (hashCode*397) ^ Domain.GetHashCode();
                return hashCode;
            }
        }

        public static Path FromString(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);

            var match = RegularExpressions.PathRegex.Match(str);

            if (!match.Success) throw new ArgumentException("The given string is not a valid SMTP path.");

            return FromMatch(match);
        }

        public static Path FromMatch(Match match)
        {
            Contract.Requires<ArgumentException>(match.Success);

            var atDomains = match.Groups["AtDomains"].Value.Split(',')
                .Where(d => !String.IsNullOrWhiteSpace(d))
                .Select(d => d.Substring(1))
                .ToArray();
            var localPart = match.Groups["LocalPart"].Value.FromSMTPString();
            var domain = match.Groups["Domain"].Value;

            return new Path(localPart, domain, atDomains);
        }
    }
}
