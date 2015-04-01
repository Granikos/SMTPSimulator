using System;
using System.Diagnostics.Contracts;

namespace HydraCore
{
    public sealed class Mailbox
    {
        public Mailbox(string localPart, string domain, string name = null)
        {
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(localPart));
            Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(domain));

            LocalPart = localPart;
            Domain = domain;
            Name = name;
        }

        public readonly string Name;
        public readonly string LocalPart;
        public readonly string Domain;

        public override string ToString()
        {
            return String.IsNullOrWhiteSpace(Name) ? String.Format("{0}@{1}", LocalPart, Domain) : String.Format("{2} <{0}@{1}>", LocalPart, Domain, Name);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var mb = obj as Mailbox;

            return mb != null && mb.LocalPart.Equals(LocalPart, StringComparison.InvariantCulture)  && mb.Domain.Equals(Domain, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}