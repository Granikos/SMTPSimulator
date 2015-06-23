using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(IExternalUserProvider))]
    public class ExternalUserProvider : DefaultProvider<ExternalUser>, IExternalUserProvider
    {
        public ExternalUserProvider()
        {
            OnAdded += entity =>
            {
                _usersByEmail.Add(entity.Mailbox, entity);
            };

            OnRemoved += entity =>
            {
                _usersByEmail.Remove(entity.Mailbox);
            };

            Add(new ExternalUser
            {
                Mailbox = "bernd.mueller@fubar.de"
            });
            Add(new ExternalUser
            {
                Mailbox = "max.muetze@fubar.de"
            });
            Add(new ExternalUser
            {
                Mailbox = "manuel.krebber@outlook.com"
            });
        }

        private readonly Dictionary<string, ExternalUser> _usersByEmail = new Dictionary<string, ExternalUser>(StringComparer.InvariantCultureIgnoreCase);

        public ExternalUser GetByEmail(string email)
        {
            ExternalUser user;
            _usersByEmail.TryGetValue(email, out user);
            return user;
        }
    }
}