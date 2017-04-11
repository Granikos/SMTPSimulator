using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    [DataContract(Name = "{0}sWithTotal")]
    public class EntitiesWithTotal<TEntity>
    {
        public EntitiesWithTotal(IEnumerable<TEntity> entities, int total)
        {
            Entities = entities;
            Total = total;
        }

        [DataMember]
        public IEnumerable<TEntity> Entities { get; set; }

        [DataMember]
        public int Total { get; set; }
    }
}