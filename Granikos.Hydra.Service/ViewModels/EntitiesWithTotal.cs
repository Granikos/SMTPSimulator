using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.ViewModels
{
    [DataContract(Name = "{0}sWithTotal")]
    public class EntitiesWithTotal<TEntity>
    {
        private readonly IEnumerable<TEntity> _entities;
        private readonly int _total;

        public EntitiesWithTotal(IEnumerable<TEntity> entities, int total)
        {
            _entities = entities;
            _total = total;
        }

        [DataMember]
        public IEnumerable<TEntity> Entities
        {
            get { return _entities; }
            set { }
        }

        [DataMember]
        public int Total
        {
            get { return _total; }
            set { }
        }
    }
}