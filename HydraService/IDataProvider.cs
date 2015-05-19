using System.Collections.Generic;

namespace HydraService
{
    public interface IDataProvider<TEntity>
    {
        IList<TEntity> All();

        TEntity Get(int id);

        TEntity Add(TEntity binding);

        TEntity Update(TEntity binding);

        bool Delete(int id);
    }
}