using System;
using System.Collections.Generic;
using System.Linq;
using HydraService.Models;

namespace HydraService.Providers
{
    public class InMemoryProvider<TEntity> : IDataProvider<TEntity>
        where TEntity : class, IEntity
    {
        private int _id = 1;

        private readonly Dictionary<int, TEntity> _bindings = new Dictionary<int, TEntity>();

        public IEnumerable<TEntity> All()
        {
            return _bindings.Values.ToList();
        }

        public TEntity Get(int id)
        {
            TEntity binding;

            _bindings.TryGetValue(id, out binding);

            return binding;
        }

        public TEntity Add(TEntity binding)
        {
            binding.Id = _id++;

            _bindings.Add(binding.Id, binding);

            return binding;
        }

        public TEntity Update(TEntity binding)
        {
            if (!_bindings.ContainsKey(binding.Id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", binding.Id, typeof(TEntity).Name));
            }

            _bindings[binding.Id] = binding;

            return binding;
        }

        public bool Delete(int id)
        {
            if (!_bindings.ContainsKey(id))
            {
                throw new ArgumentException(String.Format("The {1} with the id {0} does not exist.", id, typeof(TEntity).Name));
            }

            _bindings.Remove(id);

            return true;
        }
    }
}