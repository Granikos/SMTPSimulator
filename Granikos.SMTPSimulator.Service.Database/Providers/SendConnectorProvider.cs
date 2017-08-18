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
using System.ComponentModel.Composition;
using System.Data.Entity;
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
                if (Get(value) == null) throw new ArgumentException();

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