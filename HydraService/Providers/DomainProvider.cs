using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IDomainProvider))]
    class DomainProvider : DefaultProvider<Domain>, IDomainProvider
    {
        public DomainProvider()
        {
            Add(new Domain("test.de"));
            Add(new Domain("fubar.com"));
        }
    }
}