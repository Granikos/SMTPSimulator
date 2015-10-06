using System.Collections.Generic;

namespace Granikos.Hydra.Service.Providers
{
    public interface IDataProvider<TEntity, in TKey>
    {
        int Total { get; }
        IEnumerable<TEntity> All();
        IEnumerable<TEntity> Paged(int page, int pageSize);
        TEntity Get(TKey id);
        TEntity Add(TEntity entity);
        TEntity Update(TEntity entity);
        bool Delete(TKey id);
    }
}