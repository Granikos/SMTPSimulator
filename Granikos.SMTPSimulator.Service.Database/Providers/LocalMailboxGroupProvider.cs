﻿// MIT License
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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [Export(typeof(ILocalMailboxGroupProvider))]
    public class LocalMailboxGroupProvider : DefaultProvider<LocalUserGroup,IUserGroup>, ILocalMailboxGroupProvider<LocalUserGroup>, ILocalMailboxGroupProvider
    {
        public LocalMailboxGroupProvider() : base(LocalUserGroup.From)
        {
        }

        public LocalUserGroup Add(string name)
        {
            return Add(new LocalUserGroup {Name = name});
        }

        IUserGroup ILocalMailboxGroupProvider<IUserGroup>.Add(string name)
        {
            return Add(name);
        }

        protected override IOrderedQueryable<LocalUserGroup> ApplyOrder(IQueryable<LocalUserGroup> entities)
        {
            return entities.Include(g => g.Users).OrderBy(g => g.Name.ToLower());
        }

        protected override void OnUpdate(LocalUserGroup entity, LocalUserGroup dbEntity)
        {
            dbEntity.Users.Clear();

            var users = Database.LocalUsers.Where(u => entity.UserIds.Contains(u.Id)).ToList();

            foreach (var user in users)
            {
                dbEntity.Users.Add(user);
            }
        }
    }
}