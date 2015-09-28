using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using HydraService.Models;
using log4net;
using Newtonsoft.Json;

namespace HydraService.Providers
{
    public class DefaultProvider<TEntity> : MemoryProviderWithStorageProvider<TEntity,DataContainer<TEntity>>
        where TEntity : class, IEntity<int>
    {
        public DefaultProvider(string name = null) : base(name)
        {
        }
    }

    public class MemoryProviderWithStorageProvider<TEntity,TContainer> : InMemoryProvider<TEntity, int>
        where TEntity : class, IEntity<int>
        where TContainer : DataContainer<TEntity>, new()
    {
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        private int _id;

        public MemoryProviderWithStorageProvider(string name = null)
        {
            _serializer.Converters.Add(new IPRangeConverter());

            var fileName = (name ?? GetType().Name) + ".json";
            var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationManager.AppSettings["DataFolder"]);

            var logger = LogManager.GetLogger(GetType());

            logger.Debug("Data Folder: " + folderPath);

            if (!File.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            FileName = Path.GetFullPath(Path.Combine(folderPath, fileName));

            AutoId = entity => ++_id;

            if (!File.Exists(FileName) || !Load())
            {
                foreach (var entity in Initializer())
                {
                    Add(entity);
                }

                Store();
            }

            OnUpdated += Store;
            OnClear += () =>
            {
                _id = 0;
                Store();
            };
        }

        public string FileName { get; private set; }

        protected virtual IEnumerable<TEntity> Initializer()
        {
            return new List<TEntity>(0);
        }

        protected virtual void OnStore(TContainer container)
        {
            
        }

        protected void Store()
        {
            var container = new TContainer
            {
                Entities = All(),
                AutoId = _id
            };
            OnStore(container);

            // TODO: Make more robust
            using (var sw = new StreamWriter(FileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                _serializer.Serialize(writer, container);
            }
        }

        protected virtual void OnLoad(TContainer container)
        {

        }

        private bool Load()
        {
            using (var sr = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var container = _serializer.Deserialize<TContainer>(reader);

                if (container == null) return false;

                OnLoad(container);

                _id = container.AutoId;
                foreach (var entity in container.Entities)
                {
                    InternalAdd(entity);
                }
            }

            return true;
        }
    }
}