using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using Granikos.Hydra.Service.Database.Models;

namespace Granikos.Hydra.Service.Database
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ServiceDbContext : DbContext
    {
        protected ServiceDbContext()
        {
            System.Data.Entity.Database.SetInitializer(new DbInitializer());
        }

        public DbSet<ExternalUser> ExternalUsers { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<LocalUserGroup> LocalUserGroups { get; set; }
        public DbSet<ExternalUserGroup> ExternalUserGroups { get; set; }
        // public DbSet<MailTemplate> MailTemplateTypes { get; set; }
        public DbSet<ReceiveConnector> ReceiveConnectors { get; set; }
        public DbSet<SendConnector> SendConnectors { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<TimeTable> TimeTables { get; set; }
        public DbSet<DbIPRange> IPRanges { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AttachmentContent> AttachmentContents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalUser>().ToTable("LocalUsers");
            modelBuilder.Entity<ExternalUser>().ToTable("ExternalUsers");


            modelBuilder.Entity<LocalUserGroup>()
                .HasMany(g => g.Users)
                .WithMany(u => u.Groups);

            modelBuilder.Entity<ExternalUserGroup>()
                .HasMany(g => g.Users)
                .WithMany(u => u.Groups);

            modelBuilder.Entity<SendConnector>()
                .HasMany(s => s.InternalDomains)
                .WithRequired(d => d.Connector)
                .WillCascadeOnDelete();

            modelBuilder.Entity<ReceiveConnector>()
                .HasMany(r => r.RemoteIPRanges)
                .WithRequired(i => i.Connector)
                .WillCascadeOnDelete();
        }
    }
}
