using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(ILocalUserProvider))]
    public class LocalUserProvider : InMemoryProvider<LocalUser>, ILocalUserProvider
    {
        public LocalUserProvider()
        {
            Add(new LocalUser
                {
                    FirstName = "Bernd",
                    LastName = "Müller",
                    Mailbox = "bernd.mueller@test.de"
                });
            Add(new LocalUser
                {
                    FirstName = "Eva",
                    LastName = "Schmidt",
                    Mailbox = "eva.schmidt@test.de"
                });
        }
    }
}