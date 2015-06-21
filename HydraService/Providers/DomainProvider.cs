using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace HydraService.Providers
{
    [Export(typeof(IDomainProvider))]
    class DomainProvider : IDomainProvider
    {
        readonly HashSet<string> _domains = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public DomainProvider()
        {
            _domains.Add("test.de");
            _domains.Add("fubar.com");
        }

        public IEnumerable<string> GetDomains()
        {
            return _domains.OrderBy(s => s);
        }

        public bool Exists(string domain)
        {
            return _domains.Contains(domain);
        }

        public bool Add(string domain)
        {
            return _domains.Add(domain);
        }

        public bool Delete(string domain)
        {
            return _domains.Remove(domain);
        }
    }
}