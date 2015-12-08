using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof(IAttachmentProvider))]
    public class AttachmentProvider : IAttachmentProvider<Attachment>, IAttachmentProvider
    {
        [Import]
        protected ServiceDbContext Database;

        public int Total { get { return Database.Attachments.Count(); } }

        public IEnumerable<Attachment> All()
        {
            return Database.Attachments
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToList();
        }

        IAttachment IAttachmentProvider<IAttachment>.Get(string name)
        {
            return Get(name);
        }

        public bool Add(IAttachment entity)
        {
            var attachment = new Attachment
            {
                Name = entity.Name,
                Content = entity.Content,
                Size = entity.Size
            };

            return Add(attachment);
        }

        IEnumerable<IAttachment> IAttachmentProvider<IAttachment>.All()
        {
            return All();
        }

        public Attachment Get(string name)
        {
            return Database.Attachments
                .Include("InternalContent")
                .Single(a => a.Name == name);
        }

        public Attachment GetInternal(string name)
        {
            return Database.Attachments
                .Single(a => a.Name == name);
        }

        public bool Add(Attachment entity)
        {
            var content = new AttachmentContent { Content = entity.Content };

            entity.InternalContent = content;

            Database.AttachmentContents.Add(content);
            Database.Attachments.Add(entity);

            Database.SaveChanges();

            return true;
        }

        public bool Rename(string oldName, string newName)
        {
            var attachment = GetInternal(oldName);
            attachment.Name = newName;
            Database.SaveChanges();

            return true;
        }

        public bool Delete(string name)
        {
            var at = GetInternal(name);
            Database.Attachments.Remove(at);
            Database.SaveChanges();

            return true;
        }

        public bool Clear()
        {
            throw new NotImplementedException();
        }
    }
}