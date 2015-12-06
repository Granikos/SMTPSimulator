using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Security;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof (IReceiveConnectorProvider))]
    public class ReceiveConnectorProvider : DefaultProvider<ReceiveConnector,IReceiveConnector>, IReceiveConnectorProvider<ReceiveConnector>, IReceiveConnectorProvider
    {
        public ReceiveConnectorProvider() : base(ReceiveConnector.FromOther)
        {
        }

        public ReceiveConnector GetEmptyConnector()
        {
            return new ReceiveConnector();
        }

        IReceiveConnector IReceiveConnectorProvider<IReceiveConnector>.GetEmptyConnector()
        {
            return GetEmptyConnector();
        }
    }
}