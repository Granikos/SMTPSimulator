using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HydraService.Models;

namespace HydraService.Providers
{
    public class InMemoryProvider<TEntity> : IDataProvider<TEntity>
        where TEntity : class, IEntity
    {
        private int _id = 1;

        private readonly ConcurrentDictionary<int, TEntity> _entities = new ConcurrentDictionary<int, TEntity>();

        public IEnumerable<TEntity> All()
        {
            return _entities.Values.ToList();
        }

        public TEntity Get(int id)
        {
            TEntity binding;

            _entities.TryGetValue(id, out binding);

            return binding;
        }

        public delegate void OnAddHandler(TEntity entity);

        public event OnAddHandler OnAdd;

        public delegate void OnRemoveHandler(TEntity entity);

        public event OnRemoveHandler OnRemove;

        public TEntity Add(TEntity entity)
        {
            entity.Id = _id++;

            if (!_entities.TryAdd(entity.Id, entity))
            {
                return null;
            }

            if (OnAdd != null)
            {
                OnAdd(entity);
            }

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            if (!_entities.ContainsKey(entity.Id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", entity.Id, typeof(TEntity).Name));
            }

            if (OnRemove != null)
            {
                OnRemove(_entities[entity.Id]);
            }

            _entities[entity.Id] = entity;

            if (OnAdd != null)
            {
                OnAdd(entity);
            }

            return entity;
        }

        public bool Delete(int id)
        {
            if (!_entities.ContainsKey(id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", id, typeof(TEntity).Name));
            }

            TEntity entity;

            if (!_entities.TryRemove(id, out entity)) return false;

            if (OnRemove != null)
            {
                OnRemove(entity);
            }

            return true;
        }
    }
}