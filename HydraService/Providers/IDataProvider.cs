using System.Collections.Generic;

namespace HydraService.Providers
{
    public interface IDataProvider<TEntity, in TKey>
    {
        IEnumerable<TEntity> All();

        TEntity Get(TKey id);

        TEntity Add(TEntity binding);

        TEntity Update(TEntity binding);

        bool Delete(TKey id);
    }
}