using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Granikos.SMTPSimulator.Core
{
    public sealed class Mailbox
    {
        public readonly string Domain;
        public readonly string LocalPart;
        public readonly string Name;

        public Mailbox(string localPart, string domain, string name = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(localPart));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(domain));

            LocalPart = localPart;
            Domain = domain;
            Name = name;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name)
                ? string.Format("{0}@{1}", LocalPart, Domain)
                : string.Format("{2} <{0}@{1}>", LocalPart, Domain, Name);
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Mailbox && Equals((Mailbox) obj);
        }

        public bool Equals(Mailbox other)
        {
            if (ReferenceEquals(null, other)) return false;
            return string.Equals(LocalPart, other.LocalPart, StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(Domain, other.Domain, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}