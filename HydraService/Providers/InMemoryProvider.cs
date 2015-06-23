using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HydraService.Models;

namespace HydraService.Providers
{
    public class InMemoryProvider<TEntity, TKey> : IDataProvider<TEntity,TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        protected Func<TEntity, TKey> AutoId;

        private readonly ConcurrentDictionary<TKey, TEntity> _entities = new ConcurrentDictionary<TKey, TEntity>();

        public InMemoryProvider(Func<TEntity,TKey> autoId = null)
        {
            AutoId = autoId;
        }

        public IEnumerable<TEntity> All()
        {
            return _entities.Values.ToList();
        }

        public TEntity Get(TKey id)
        {
            TEntity binding;

            _entities.TryGetValue(id, out binding);

            return binding;
        }

        public delegate void PostHandler(TEntity entity);

        public delegate bool PreHandler(TKey id);

        public event PostHandler OnAdded;

        public event PostHandler OnRemoved;

        protected virtual bool CanRemove(TKey id)
        {
            return true;
        }

        public TEntity Add(TEntity entity)
        {
            if (AutoId != null)
            {
                entity.Id = AutoId(entity);
            }
            else if (entity.Id == null)
            {
                throw new ArgumentException("Entity id not set.", "entity");
            }

            if (!_entities.TryAdd(entity.Id, entity))
            {
                return null;
            }

            if (OnAdded != null)
            {
                OnAdded(entity);
            }

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            if (!_entities.ContainsKey(entity.Id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", entity.Id, typeof(TEntity).Name));
            }

            if (OnRemoved != null)
            {
                OnRemoved(_entities[entity.Id]);
            }

            _entities[entity.Id] = entity;

            if (OnAdded != null)
            {
                OnAdded(entity);
            }

            return entity;
        }

        public bool Delete(TKey id)
        {
            if (!_entities.ContainsKey(id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", id, typeof(TEntity).Name));
            }

            if (!CanRemove(id)) return false;

            TEntity entity;

            if (!_entities.TryRemove(id, out entity)) return false;

            if (OnRemoved != null)
            {
                OnRemoved(entity);
            }

            return true;
        }
    }
}