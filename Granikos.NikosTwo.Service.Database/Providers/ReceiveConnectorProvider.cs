using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using Granikos.NikosTwo.Service.Database.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service.Database.Providers
{
    [Export(typeof (IReceiveConnectorProvider))]
    public class ReceiveConnectorProvider : DefaultProvider<ReceiveConnector,IReceiveConnector>, IReceiveConnectorProvider<ReceiveConnector>, IReceiveConnectorProvider
    {
        public ReceiveConnectorProvider() : base(ReceiveConnector.FromOther)
        {
        }

        public ReceiveConnector GetEmptyConnector()
        {
            return new ReceiveConnector();
        }

        IReceiveConnector IReceiveConnectorProvider<IReceiveConnector>.GetEmptyConnector()
        {
            return GetEmptyConnector();
        }

        protected override void OnUpdate(ReceiveConnector entity, ReceiveConnector dbEntity)
        {
            foreach (var range in dbEntity.RemoteIPRanges.ToArray())
            {
                Database.Entry(range).State = EntityState.Deleted;
            }

            dbEntity.RemoteIPRanges.Clear();

            foreach (var range in entity.RemoteIPRanges)
            {
                dbEntity.RemoteIPRanges.Add(new DbIPRange(range.Start, range.End));
            }
        }
    }
}