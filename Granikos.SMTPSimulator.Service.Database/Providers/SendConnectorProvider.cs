using System;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [Export(typeof(ISendConnectorProvider))]
    public class SendConnectorProvider : DefaultProvider<SendConnector, ISendConnector>, ISendConnectorProvider<SendConnector>, ISendConnectorProvider
    {
        public SendConnectorProvider()
            : base(ModelHelpers.ConvertTo<SendConnector>)
        {
        }

        public int DefaultId
        {
            get
            {
                return DefaultConnector.Id;
            }

            set
            {
                Contract.Requires<ArgumentException>(Get(value) != null);

                var newDefault = Database.SendConnectors.Find(value);

                if (newDefault == null)
                {
                    throw new ArgumentException("Invalid send connector id " + value);
                }

                DefaultConnector.Default = false;
                newDefault.Default = true;
                Database.SaveChanges();
            }
        }

        public SendConnector DefaultConnector
        {
            get { return Database.SendConnectors.Single(s => s.Default); }
        }

        ISendConnector ISendConnectorProvider<ISendConnector>.GetByDomain(string domain)
        {
            return GetByDomain(domain);
        }

        ISendConnector ISendConnectorProvider<ISendConnector>.GetEmptyConnector()
        {
            return GetEmptyConnector();
        }

        public SendConnector GetEmptyConnector()
        {
            return new SendConnector();
        }

        ISendConnector ISendConnectorProvider<ISendConnector>.DefaultConnector
        {
            get { return DefaultConnector; }
        }

        public SendConnector GetByDomain(string domain)
        {
            return DefaultConnector;
            // TODO
        }

        public override bool CanRemove(int id)
        {
            return id != DefaultId;
        }

        protected override void OnUpdate(SendConnector entity, SendConnector dbEntity)
        {
            Database.Entry(dbEntity).Property(s => s.Default).IsModified = false;

            foreach (var domain in dbEntity.InternalDomains.ToArray())
            {
                Database.Entry(domain).State = EntityState.Deleted;
            }

            dbEntity.InternalDomains.Clear();

            foreach (var domain in entity.Domains)
            {
                dbEntity.InternalDomains.Add(new Domain { DomainName = domain });
            }
        }
    }
}