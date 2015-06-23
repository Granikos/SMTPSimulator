using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(ISendConnectorProvider))]
    public class SendConnectorProvider : DefaultProvider<SendConnector>, ISendConnectorProvider
    {
        public SendConnectorProvider()
        {
            Add(new SendConnector
            {
                Name = "Default"
            });
        }

        protected override bool CanRemove(int id)
        {
            return id != _defaultId;
        }

        private int _defaultId = 1;

        public int DefaultId
        {
            get { return _defaultId; }
            set
            {
                Contract.Requires<ArgumentException>(Get(value) != null);

                _defaultId = value;
            }
        }

        public SendConnector DefaultConnector { get { return Get(DefaultId); } }
    }
}