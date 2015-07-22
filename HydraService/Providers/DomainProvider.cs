using System.Collections.Generic;
using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IDomainProvider))]
    class DomainProvider : DefaultProvider<Domain>, IDomainProvider
    {
        public DomainProvider()
            : base("Domains")
        {
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