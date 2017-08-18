// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
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