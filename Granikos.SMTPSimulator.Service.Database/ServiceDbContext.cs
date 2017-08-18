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
using System.ComponentModel.Composition;
using System.Data.Entity;
using Granikos.SMTPSimulator.Service.Database.Migrations;
using Granikos.SMTPSimulator.Service.Database.Models;

namespace Granikos.SMTPSimulator.Service.Database
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ServiceDbContext : DbContext
    {
        public ServiceDbContext()
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<ServiceDbContext, Configuration>());
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
        public DbSet<TimeTableParameter> TimeTableParameters { get; set; }
        public DbSet<DbIPRange> IPRanges { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AttachmentContent> AttachmentContents { get; set; }
        public DbSet<MailTemplate> MailTemplates { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CertificateContent> CertificateContents { get; set; }

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

            modelBuilder.Entity<TimeTable>()
                .HasMany(t => t.InternalParameters)
                .WithRequired(p => p.TimeTable)
                .HasForeignKey(p => p.TimeTableId)
                .WillCascadeOnDelete(true);
        }
    }
}
