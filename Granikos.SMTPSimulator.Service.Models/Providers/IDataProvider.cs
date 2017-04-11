using System.Collections.Generic;

namespace Granikos.SMTPSimulator.Service.Models.Providers
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

        bool Validate(TEntity entity, out string message);

        bool CanRemove(TKey key);

        bool Clear();
    }
}