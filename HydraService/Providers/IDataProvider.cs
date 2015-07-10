using System.Collections.Generic;

namespace HydraService.Providers
{
    public interface IDataProvider<TEntity, in TKey>
    {
        int Total { get; }

        IEnumerable<TEntity> All();

        IEnumerable<TEntity> Paged(int page, int pageSize);

        TEntity Get(TKey id);

        TEntity Add(TEntity binding);

        TEntity Update(TEntity binding);

        bool Delete(TKey id);
    }
}