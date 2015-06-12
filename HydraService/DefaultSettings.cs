using HydraCore;
using HydraService.Models;

namespace HydraService
{
    public class DefaultSettings : ISettings
    {
        private readonly RecieveConnector _connector;

        public DefaultSettings(RecieveConnector connector)
        {
            _connector = connector;
        }

        public string Banner { get { return _connector.Banner; } }

        public string Greet { get { return _connector.Banner; } }

        public bool RequireAuth { get { return _connector.RequireAuth; } }

        public bool RequireTLS { get { return _connector.RequireTLS; } }
    }
}