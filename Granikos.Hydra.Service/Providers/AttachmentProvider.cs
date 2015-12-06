using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof(IAttachmentProvider))]
    public class AttachmentProvider : IAttachmentProvider
    {
        public int[] GetAttachmentIds()
        {
            throw new NotImplementedException();
        }

        public byte[] GetAttachmentContent(int id)
        {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(IMailTemplateProvider))]
    public class MailTemplateProvider : IMailTemplateProvider
    {
        public IEnumerable<IMailTemplate> GetMailTemplates()
        {
            throw new NotImplementedException();
        }
    }
}
