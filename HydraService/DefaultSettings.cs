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

        public bool RequireTLS { get { return _connector.TLSSettings.Mode == TLSMode.Required; } }
        public bool EnableTLS { get { return _connector.TLSSettings.Mode != TLSMode.Disabled && _connector.TLSSettings.Mode != TLSMode.FullTunnel; } }
    }
}