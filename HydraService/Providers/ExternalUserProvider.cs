using System.Collections.Generic;
using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IExternalUserProvider))]
    public class ExternalUserProvider : InMemoryProvider<ExternalUser>, IExternalUserProvider
    {
        public ExternalUserProvider()
        {
            Add(new ExternalUser
            {
                Mailbox = "bernd.mueller@fubar.de"
            });
            Add(new ExternalUser
            {
                Mailbox = "max.muetze@fubar.de"
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

        private readonly Dictionary<string, ExternalUser> _usersByEmail = new Dictionary<string, ExternalUser>();

        public ExternalUser GetByEmail(string email)
        {
            ExternalUser user;
            _usersByEmail.TryGetValue(email, out user);
            return user;
        }
    }
}