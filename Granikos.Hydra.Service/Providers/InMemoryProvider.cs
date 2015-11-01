using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public class InMemoryProvider<TEntity, TKey> : IDataProvider<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        public delegate void PostHandler(TEntity entity);

        public delegate bool PreHandler(TKey id);

        public delegate void UpdateHandler();

        private readonly ConcurrentDictionary<TKey, TEntity> _entities = new ConcurrentDictionary<TKey, TEntity>();
        protected Func<TEntity, TKey> AutoId;

        public InMemoryProvider(Func<TEntity, TKey> autoId = null)
        {
            AutoId = autoId;
        }

        public int Total
        {
            get { return _entities.Count; }
        }

        public IEnumerable<TEntity> All()
        {
            return _entities.Values.ToList();
        }

        protected virtual bool Validate(TEntity entity, out string message)
        {
            var context = new ValidationContext(entity, null, null);
            var results = new List<ValidationResult>();
            var success = Validator.TryValidateObject(entity, context, results, true);

            message = results.Select(r => r.ErrorMessage).FirstOrDefault();

            return success;
        }

        public IEnumerable<TEntity> Paged(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var entities = ApplyOrder(_entities.Values);
            return entities.Skip(skip).Take(pageSize).ToList();
        }

        public TEntity Get(TKey id)
        {
            TEntity entity;

            _entities.TryGetValue(id, out entity);

            return entity;
        }

        public TEntity Add(TEntity entity)
        {
            string message;
            if (!Validate(entity, out message))
            {
                throw new ValidationException(message ?? "Validation failed for entity.");
            }

            if (AutoId != null)
            {
                entity.Id = AutoId(entity);
            }

            if (entity.Id == null)
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

            if (OnUpdated != null)
            {
                OnUpdated();
            }

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            if (!_entities.ContainsKey(entity.Id))
            {
                throw new ArgumentException(string.Format("The {1} with the id {0} does not exist.", entity.Id,
                    typeof(TEntity).Name));
            }

            string message;
            if (!Validate(entity, out message))
            {
                throw new ValidationException(message ?? "Validation failed for entity.");
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

            if (OnUpdated != null)
            {
                OnUpdated();
            }

            return entity;
        }

        public bool Delete(TKey id)
        {
            if (!_entities.ContainsKey(id))
            {
                throw new ArgumentException(string.Format("The {1} with the id {0} does not exist.", id,
                    typeof(TEntity).Name));
            }

            if (!CanRemove(id)) return false;

            TEntity entity;

            if (!_entities.TryRemove(id, out entity)) return false;

            if (OnRemoved != null)
            {
                OnRemoved(entity);
            }

            if (OnUpdated != null)
            {
                OnUpdated();
            }

            return true;
        }

        protected void Clear()
        {
            _entities.Clear();

            if (OnClear != null)
            {
                OnClear();
            }
        }

        protected virtual IOrderedEnumerable<TEntity> ApplyOrder(IEnumerable<TEntity> entities)
        {
            return entities.OrderBy(e => e.Id);
        }

        public event PostHandler OnAdded;
        public event PostHandler OnRemoved;
        public event UpdateHandler OnUpdated;
        public event UpdateHandler OnClear;

        protected virtual bool CanRemove(TKey id)
        {
            return true;
        }

        protected bool InternalAdd(TEntity entity)
        {
            if (entity.Id == null)
            {
                throw new ArgumentException("Entity id not set.", "entity");
            }

            return _entities.TryAdd(entity.Id, entity);
        }
    }
}