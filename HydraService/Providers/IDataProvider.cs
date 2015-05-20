using System.Collections.Generic;

namespace HydraService.Providers
{
    public interface IDataProvider<TEntity>
    {
        IEnumerable<TEntity> All();

        TEntity Get(int id);

        TEntity Add(TEntity binding);

        TEntity Update(TEntity binding);

        bool Delete(int id);
    }
}