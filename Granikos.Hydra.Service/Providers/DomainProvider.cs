using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof (IDomainProvider))]
    internal class DomainProvider : DefaultProvider<Domain>, IDomainProvider
    {
        private readonly Dictionary<string, int> _idByDomain = new Dictionary<string, int>();

        public DomainProvider()
            : base("Domains")
        {
        }

        public Domain GetByName(string name)
        {
            return All().FirstOrDefault(d => d.DomainName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

#if DEBUG
        protected override IEnumerable<Domain> Initializer()
        {
            yield return new Domain("test.de");
            yield return new Domain("fubar.com");
        }
#endif
    }
}