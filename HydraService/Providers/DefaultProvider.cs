using HydraService.Models;

namespace HydraService.Providers
{
    public class DefaultProvider<TEntity> : InMemoryProvider<TEntity,int>
        where TEntity : class, IEntity<int>
    {
        private int _id = 0;

        public DefaultProvider()
        {
            AutoId = entity => ++_id;
        }
    }
}