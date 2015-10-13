using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public class SendConnectorContainer : DataContainer<SendConnector>
    {
        [DataMember]
        public int DefaultId { get; set; }
    }

    [Export(typeof (ISendConnectorProvider))]
    public class SendConnectorProvider : MemoryProviderWithStorageProvider<SendConnector, SendConnectorContainer>,
        ISendConnectorProvider
    {
        private int _defaultId = 1;

        public SendConnectorProvider()
            : base("SendConnectors")
        {
        }

        public int DefaultId
        {
            get { return _defaultId; }
            set
            {
                Contract.Requires<ArgumentException>(Get(value) != null);

                _defaultId = value;

                Store();
            }
        }

        public SendConnector DefaultConnector
        {
            get { return Get(DefaultId); }
        }

        public SendConnector GetByDomain(string domain)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<SendConnector> Initializer()
        {
            yield return new SendConnector
            {
                Name = "Default",
                UseSmarthost = false,
                RetryTime = TimeSpan.FromMinutes(5),
                RetryCount = 3
            };
        }

        protected override bool CanRemove(int id)
        {
            return id != _defaultId;
        }

        protected override void OnLoad(SendConnectorContainer container)
        {
            _defaultId = container.DefaultId;
        }

        protected override void OnStore(SendConnectorContainer container)
        {
            container.DefaultId = _defaultId;
        }
    }
}