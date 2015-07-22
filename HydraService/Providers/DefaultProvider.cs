using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using HydraService.Models;
using Newtonsoft.Json;

namespace HydraService.Providers
{
    [DataContract]
    class DataContainer<TEntity>
    {
        [DataMember]
        public IEnumerable<TEntity> Entities { get; set; }

        [DataMember]
        public int AutoId { get; set; }
    }

    public class DefaultProvider<TEntity> : InMemoryProvider<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        private int _id = 0;

        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public DefaultProvider(string name = null)
        {
            var fileName = (name ?? GetType().Name) + ".json";
            FileName = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["DataFolder"], fileName));

            if (!File.Exists(FileName))
            {
                foreach (var entity in Initializer())
                {
                    Add(entity);
                }

                Store();
            }
            else
            {
                Load();
            }

            AutoId = entity => ++_id;

            OnUpdated += Store;
        }

        protected virtual IEnumerable<TEntity> Initializer()
        {
            return new List<TEntity>(0);
        }

        private void Store()
        {
            // TODO: Make more robust
            using (StreamWriter sw = new StreamWriter(FileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                _serializer.Serialize(writer, new DataContainer<TEntity>
                {
                    Entities = All(),
                    AutoId = _id
                });
            }
        }

        private void Load()
        {
            using (StreamReader sr = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var container = _serializer.Deserialize<DataContainer<TEntity>>(reader);

                _id = container.AutoId;
                foreach (var entity in container.Entities)
                {
                    InternalAdd(entity);
                }
            }
        }

        public string FileName { get; private set; }
    }
}