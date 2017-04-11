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