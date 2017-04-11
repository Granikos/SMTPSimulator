using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database
{
    public class DatabaseProvider<TEntity, TKey> : IDataProvider<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        [Import]
        protected ServiceDbContext Database;

        public int Total { get { return Database.Set<TEntity>().Count(); } }

        protected virtual DbSet<TEntity> Set { get { return Database.Set<TEntity>(); } }

        public IEnumerable<TEntity> All()
        {
            return ApplyOrder(Set).AsNoTracking();
        }

        public IEnumerable<TEntity> Paged(int page, int pageSize)
        {
            var skip = Math.Max((page - 1)*pageSize, 0);
            return ApplyOrder(Set).AsNoTracking().Skip(skip).Take(pageSize);
        }

        public TEntity Get(TKey id)
        {
            var entity = Set.Find(id);

            return entity;
        }

        public TEntity Add(TEntity entity)
        {
            Set.Add(entity);

            OnAdd(entity);

            try
            {
                Database.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }

            OnAdded(entity);

            return entity;
        }

        protected virtual void OnAdd(TEntity entity)
        {
        }

        protected virtual void OnUpdate(TEntity entity, TEntity dbEntity)
        {

        }

        protected virtual void OnAdded(TEntity entity)
        {
        }

        protected virtual void OnUpdated(TEntity entity)
        {
        }

        protected virtual void OnDeleted(TEntity entity)
        {
        }

        public TEntity Update(TEntity entity)
        {
            var dbEntity = Set.Find(entity.Id);

            Database.Entry(dbEntity).CurrentValues.SetValues(entity);

            OnUpdate(entity, dbEntity);

            try
            {
                Database.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                throw;
            }

            OnUpdated(dbEntity);

            return dbEntity;
        }

        public bool Delete(TKey id)
        {
            var entity = Database.Set<TEntity>().Remove(Get(id));

            Database.SaveChanges();

            OnDeleted(entity);

            return true;
        }

        public virtual bool Validate(TEntity entity, out string message)
        {
            message = null;
            return true;
        }

        public virtual bool CanRemove(TKey key)
        {
            return true;
        }

        public bool Clear()
        {
            Database.Set<TEntity>().RemoveRange(Database.Set<TEntity>());

            Database.SaveChanges();

            return true;
        }

        protected virtual IOrderedQueryable<TEntity> ApplyOrder(IQueryable<TEntity> entities)
        {
            return entities.OrderBy(e => e.Id);
        }
    }
}