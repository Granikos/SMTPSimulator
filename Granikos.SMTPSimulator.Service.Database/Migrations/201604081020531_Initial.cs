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
namespace Granikos.SMTPSimulator.Service.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachmentContents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Attachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 450),
                        Size = c.Int(nullable: false),
                        InternalContent_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttachmentContents", t => t.InternalContent_Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.InternalContent_Id);
            
            CreateTable(
                "dbo.CertificateContents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Certificates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 450),
                        InternalContent_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CertificateContents", t => t.InternalContent_Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.InternalContent_Id);
            
            CreateTable(
                "dbo.Domains",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectorId = c.Int(nullable: false),
                        DomainName = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SendConnectors", t => t.ConnectorId, cascadeDelete: true)
                .Index(t => t.ConnectorId);
            
            CreateTable(
                "dbo.SendConnectors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Default = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 4000),
                        LocalAddressString = c.String(nullable: false, maxLength: 4000),
                        RemoteAddressString = c.String(maxLength: 4000),
                        RemotePort = c.Int(nullable: false),
                        UseSmarthost = c.Boolean(nullable: false),
                        TLSSettings_Mode = c.Int(nullable: false),
                        TLSSettings_SslProtocols = c.Int(nullable: false),
                        TLSSettings_EncryptionPolicy = c.Int(nullable: false),
                        TLSSettings_AuthLevel = c.Int(nullable: false),
                        TLSSettings_ValidateCertificateRevocation = c.Boolean(nullable: false),
                        TLSSettings_CertificateName = c.String(maxLength: 4000),
                        TLSSettings_CertificatePassword = c.String(maxLength: 4000),
                        TLSSettings_CertificateDomain = c.String(maxLength: 4000),
                        TLSSettings_CertificateType = c.String(maxLength: 4000),
                        RetryTimeInternal = c.Int(nullable: false),
                        RetryCount = c.Int(nullable: false),
                        UseAuth = c.Boolean(nullable: false),
                        Username = c.String(maxLength: 4000),
                        Password = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExternalUserGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExternalUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(maxLength: 4000),
                        LastName = c.String(maxLength: 4000),
                        Mailbox = c.String(nullable: false, maxLength: 450),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Mailbox);
            
            CreateTable(
                "dbo.DbIPRanges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartString = c.String(maxLength: 4000),
                        EndString = c.String(maxLength: 4000),
                        Connector_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReceiveConnectors", t => t.Connector_Id, cascadeDelete: true)
                .Index(t => t.Connector_Id);
            
            CreateTable(
                "dbo.ReceiveConnectors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 4000),
                        AddressString = c.String(nullable: false, maxLength: 4000),
                        Port = c.Int(nullable: false),
                        Banner = c.String(nullable: false, maxLength: 4000),
                        RequireAuth = c.Boolean(nullable: false),
                        AuthUsername = c.String(maxLength: 4000),
                        AuthPassword = c.String(maxLength: 4000),
                        TLSSettings_Mode = c.Int(nullable: false),
                        TLSSettings_SslProtocols = c.Int(nullable: false),
                        TLSSettings_EncryptionPolicy = c.Int(nullable: false),
                        TLSSettings_AuthLevel = c.Int(nullable: false),
                        TLSSettings_ValidateCertificateRevocation = c.Boolean(nullable: false),
                        TLSSettings_CertificateName = c.String(maxLength: 4000),
                        TLSSettings_CertificatePassword = c.String(maxLength: 4000),
                        TLSSettings_CertificateDomain = c.String(maxLength: 4000),
                        TLSSettings_CertificateType = c.String(maxLength: 4000),
                        GreylistingTimeInternal = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocalUserGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocalUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(maxLength: 4000),
                        LastName = c.String(maxLength: 4000),
                        Mailbox = c.String(nullable: false, maxLength: 450),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Mailbox);
            
            CreateTable(
                "dbo.MailTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(maxLength: 4000),
                        Subject = c.String(maxLength: 4000),
                        HeaderEncoding = c.Int(nullable: false),
                        SubjectEncoding = c.Int(nullable: false),
                        BodyEncoding = c.Int(nullable: false),
                        Html = c.String(maxLength: 4000),
                        Text = c.String(maxLength: 4000),
                        Behaviour = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TimeTableParameters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeTableId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        Value = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TimeTables", t => t.TimeTableId, cascadeDelete: true)
                .Index(t => new { t.Name, t.TimeTableId }, unique: true, name: "KeyIndex");
            
            CreateTable(
                "dbo.TimeTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 4000),
                        Active = c.Boolean(nullable: false),
                        RecipientMailbox = c.String(maxLength: 4000),
                        RecipientGroupId = c.Int(nullable: false),
                        StaticRecipient = c.Boolean(nullable: false),
                        SenderMailbox = c.String(maxLength: 4000),
                        SenderGroupId = c.Int(nullable: false),
                        StaticSender = c.Boolean(nullable: false),
                        MinRecipients = c.Int(nullable: false),
                        MaxRecipients = c.Int(nullable: false),
                        MailTemplateId = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 4000),
                        ReportMailAddress = c.String(maxLength: 4000),
                        ReportType = c.Int(nullable: false),
                        ProtocolLevel = c.Int(nullable: false),
                        AttachmentName = c.String(maxLength: 4000),
                        AttachmentType = c.Int(nullable: false),
                        SendEicarFile = c.Boolean(nullable: false),
                        MailsSuccess = c.Int(nullable: false),
                        MailsError = c.Int(nullable: false),
                        ActiveSince = c.DateTime(),
                        ActiveUntil = c.DateTime(),
                        ReportHour = c.Int(nullable: false),
                        ReportDay = c.Int(nullable: false),
                        SendConnector_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SendConnectors", t => t.SendConnector_Id)
                .Index(t => t.SendConnector_Id);
            
            CreateTable(
                "dbo.ExternalUserGroupExternalUsers",
                c => new
                    {
                        ExternalUserGroup_Id = c.Int(nullable: false),
                        ExternalUser_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ExternalUserGroup_Id, t.ExternalUser_Id })
                .ForeignKey("dbo.ExternalUserGroups", t => t.ExternalUserGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.ExternalUsers", t => t.ExternalUser_Id, cascadeDelete: true)
                .Index(t => t.ExternalUserGroup_Id)
                .Index(t => t.ExternalUser_Id);
            
            CreateTable(
                "dbo.LocalUserGroupLocalUsers",
                c => new
                    {
                        LocalUserGroup_Id = c.Int(nullable: false),
                        LocalUser_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LocalUserGroup_Id, t.LocalUser_Id })
                .ForeignKey("dbo.LocalUserGroups", t => t.LocalUserGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.LocalUsers", t => t.LocalUser_Id, cascadeDelete: true)
                .Index(t => t.LocalUserGroup_Id)
                .Index(t => t.LocalUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TimeTables", "SendConnector_Id", "dbo.SendConnectors");
            DropForeignKey("dbo.TimeTableParameters", "TimeTableId", "dbo.TimeTables");
            DropForeignKey("dbo.LocalUserGroupLocalUsers", "LocalUser_Id", "dbo.LocalUsers");
            DropForeignKey("dbo.LocalUserGroupLocalUsers", "LocalUserGroup_Id", "dbo.LocalUserGroups");
            DropForeignKey("dbo.DbIPRanges", "Connector_Id", "dbo.ReceiveConnectors");
            DropForeignKey("dbo.ExternalUserGroupExternalUsers", "ExternalUser_Id", "dbo.ExternalUsers");
            DropForeignKey("dbo.ExternalUserGroupExternalUsers", "ExternalUserGroup_Id", "dbo.ExternalUserGroups");
            DropForeignKey("dbo.Domains", "ConnectorId", "dbo.SendConnectors");
            DropForeignKey("dbo.Certificates", "InternalContent_Id", "dbo.CertificateContents");
            DropForeignKey("dbo.Attachments", "InternalContent_Id", "dbo.AttachmentContents");
            DropIndex("dbo.LocalUserGroupLocalUsers", new[] { "LocalUser_Id" });
            DropIndex("dbo.LocalUserGroupLocalUsers", new[] { "LocalUserGroup_Id" });
            DropIndex("dbo.ExternalUserGroupExternalUsers", new[] { "ExternalUser_Id" });
            DropIndex("dbo.ExternalUserGroupExternalUsers", new[] { "ExternalUserGroup_Id" });
            DropIndex("dbo.TimeTables", new[] { "SendConnector_Id" });
            DropIndex("dbo.TimeTableParameters", "KeyIndex");
            DropIndex("dbo.LocalUsers", new[] { "Mailbox" });
            DropIndex("dbo.DbIPRanges", new[] { "Connector_Id" });
            DropIndex("dbo.ExternalUsers", new[] { "Mailbox" });
            DropIndex("dbo.Domains", new[] { "ConnectorId" });
            DropIndex("dbo.Certificates", new[] { "InternalContent_Id" });
            DropIndex("dbo.Certificates", new[] { "Name" });
            DropIndex("dbo.Attachments", new[] { "InternalContent_Id" });
            DropIndex("dbo.Attachments", new[] { "Name" });
            DropTable("dbo.LocalUserGroupLocalUsers");
            DropTable("dbo.ExternalUserGroupExternalUsers");
            DropTable("dbo.TimeTables");
            DropTable("dbo.TimeTableParameters");
            DropTable("dbo.MailTemplates");
            DropTable("dbo.LocalUsers");
            DropTable("dbo.LocalUserGroups");
            DropTable("dbo.ReceiveConnectors");
            DropTable("dbo.DbIPRanges");
            DropTable("dbo.ExternalUsers");
            DropTable("dbo.ExternalUserGroups");
            DropTable("dbo.SendConnectors");
            DropTable("dbo.Domains");
            DropTable("dbo.Certificates");
            DropTable("dbo.CertificateContents");
            DropTable("dbo.Attachments");
            DropTable("dbo.AttachmentContents");
        }
    }
}
