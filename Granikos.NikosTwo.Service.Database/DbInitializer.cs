using System;
using System.Data.Entity;
using System.Net;
using System.Net.Security;
using Granikos.NikosTwo.Service.Database.Models;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.Database
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<ServiceDbContext>
    {
        protected override void Seed(ServiceDbContext context)
        {
            base.Seed(context);

            context.SendConnectors.Add(new SendConnector
            {
                Name = "Default",
                UseSmarthost = false,
                RetryTime = TimeSpan.FromMinutes(5),
                RetryCount = 3,
                Default = true
            });

            context.ReceiveConnectors.Add(new ReceiveConnector
            {
                Name = "Default",
                Enabled = true,
                Address = IPAddress.Parse("0.0.0.0"),
                Port = 25,
                Banner = "nikos two ready DEFAULT",
                TLSSettings = new TLSSettings
                {
                    EncryptionPolicy = EncryptionPolicy.NoEncryption
                },
                GreylistingTime = TimeSpan.FromMinutes(15)
            });

            context.SaveChanges();
        }
    }
}