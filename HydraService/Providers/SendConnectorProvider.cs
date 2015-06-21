using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(ISendConnectorProvider))]
    public class SendConnectorProvider : InMemoryProvider<SendConnector>, ISendConnectorProvider
    {
        public SendConnectorProvider()
        {
            Add(new SendConnector
            {
                Name = "Default",
                Domains = new [] {"*"}
            });
        }
    }
}