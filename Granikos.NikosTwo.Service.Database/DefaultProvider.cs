using System;
using System.Collections.Generic;
using System.Linq;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service.Database
{
    public class DefaultProvider<TEntity, TInterface> : DatabaseProvider<TEntity, int>, IDataProvider<TInterface,int>
        where TEntity : class, TInterface, new()
        where TInterface : IEntity<int>
    {
        private readonly Func<TInterface, TEntity> _converter;

        public DefaultProvider(Func<TInterface, TEntity> converter)
        {
            _converter = converter;
        }

        IEnumerable<TInterface> IDataProvider<TInterface, int>.All()
        {
            return All().Select(entity => (TInterface) entity);
        }

        IEnumerable<TInterface> IDataProvider<TInterface, int>.Paged(int page, int pageSize)
        {
            return Paged(page, pageSize).Select(entity => (TInterface)entity);
        }

        TInterface IDataProvider<TInterface, int>.Get(int id)
        {
            return Get(id);
        }

        TInterface IDataProvider<TInterface, int>.Add(TInterface entity)
        {
            return Add(_converter(entity));
        }

        TInterface IDataProvider<TInterface, int>.Update(TInterface entity)
        {
            return Update(_converter(entity));
        }

        bool IDataProvider<TInterface, int>.Validate(TInterface entity, out string message)
        {
            return Validate(_converter(entity), out message);
        }
    }
}