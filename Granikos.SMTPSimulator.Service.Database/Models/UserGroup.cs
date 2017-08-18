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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public abstract class UserGroup : Entity, IUserGroup
    {
        protected UserGroup(string name)
        {
            Name = name;
        }

        protected UserGroup()
        {
        }

        [Required]
        public string Name { get; set; }

        public abstract int[] UserIds { get; set; }
    }

    public class LocalUserGroup : UserGroup
    {
        private ICollection<LocalUser> _users = new List<LocalUser>();

        public LocalUserGroup(string name) : base(name)
        {
        }

        public LocalUserGroup()
        {
        }

        public static LocalUserGroup From(IUserGroup source)
        {
            var group = new LocalUserGroup();

            source.CopyTo(group);

            return group;
        }

        public virtual ICollection<LocalUser> Users
        {
            get { return _users; }
            set { _users = (value ?? new List<LocalUser>()); }
        }

        public override int[] UserIds
        {
            get { return Users.Select(u => u.Id).ToArray(); }
            set { Users = value.Select(u => new LocalUser {Id = u}).ToList(); }
        }
    }

    public class ExternalUserGroup : UserGroup
    {
        private ICollection<ExternalUser> _users = new List<ExternalUser>();

        public ExternalUserGroup(string name) : base(name)
        {
        }

        public ExternalUserGroup()
        {
        }

        public static ExternalUserGroup From(IUserGroup source)
        {
            var group = new ExternalUserGroup();

            source.CopyTo(group);

            return group;
        }

        public virtual ICollection<ExternalUser> Users
        {
            get { return _users; }
            set { _users = (value ?? new List<ExternalUser>()); }
        }

        public override int[] UserIds
        {
            get { return Users.Select(u => u.Id).ToArray(); }
            set { Users = value.Select(u => new ExternalUser { Id = u }).ToList(); }
        }
    }
}