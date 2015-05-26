using System.Collections.Generic;
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

            OnAdd += entity =>
            {
                _usersByEmail.Add(entity.Mailbox, entity);
            };

            OnRemove += entity =>
            {
                _usersByEmail.Remove(entity.Mailbox);
            };
        }

        private readonly Dictionary<string, LocalUser> _usersByEmail = new Dictionary<string, LocalUser>();

        public LocalUser GetByEmail(string email)
        {
            return _usersByEmail[email];
        }
    }
}