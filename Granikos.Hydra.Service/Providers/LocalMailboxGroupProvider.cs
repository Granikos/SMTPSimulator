using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof (ILocalMailboxGroupProvider))]
    internal class LocalMailboxGroupProvider : DefaultProvider<MailboxGroup>, ILocalMailboxGroupProvider
    {
        public LocalMailboxGroupProvider()
            : base("LocalGroups")
        {
        }

#if DEBUG
        protected override IEnumerable<MailboxGroup> Initializer()
        {
            yield return new MailboxGroup("Group 1");
            yield return new MailboxGroup("Group 2");
        }
#endif
    }

    [Export(typeof(IExternalMailboxGroupProvider))]
    internal class ExternalMailboxGroupProvider : DefaultProvider<MailboxGroup>, IExternalMailboxGroupProvider
    {
        public ExternalMailboxGroupProvider()
            : base("ExternalGroups")
        {
        }

#if DEBUG
        protected override IEnumerable<MailboxGroup> Initializer()
        {
            yield return new MailboxGroup("Group 1");
            yield return new MailboxGroup("Group 2");
        }
#endif
    }
}