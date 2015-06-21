using HydraCore;
using HydraService.Models;

namespace HydraService
{
    public class DefaultSendSettings : ISendSettings
    {
        private readonly SendConnector _connector;

        public DefaultSendSettings(SendConnector connector)
        {
            _connector = connector;
        }

        public bool UseAuth
        {
            get { return _connector.UseAuth; }
        }

        public string Username
        {
            get { return _connector.Username; }
        }

        public string Password
        {
            get { return _connector.Password; }
        }

        public bool RequireTLS
        {
            get { return _connector.TLSSettings.Mode == TLSMode.Required; }
        }

        public bool EnableTLS
        {
            get
            {
                return _connector.TLSSettings.Mode != TLSMode.Disabled &&
                       _connector.TLSSettings.Mode != TLSMode.FullTunnel;
            }
        }
    }
}