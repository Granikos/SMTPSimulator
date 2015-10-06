using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Providers
{
    [DataContract]
    public class DataContainer<TEntity>
    {
        [DataMember]
        public IEnumerable<TEntity> Entities { get; set; }

        [DataMember]
        public int AutoId { get; set; }
    }
}