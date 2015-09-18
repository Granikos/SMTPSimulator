using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using HydraService.Models;
using log4net;
using Newtonsoft.Json;

namespace HydraService.Providers
{
    [DataContract]
    internal class DataContainer<TEntity>
    {
        [DataMember]
        public IEnumerable<TEntity> Entities { get; set; }

        [DataMember]
        public int AutoId { get; set; }
    }

    public class DefaultProvider<TEntity> : InMemoryProvider<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        private int _id;

        public DefaultProvider(string name = null)
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

        private void Store()
        {
            // TODO: Make more robust
            using (var sw = new StreamWriter(FileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                _serializer.Serialize(writer, new DataContainer<TEntity>
                {
                    Entities = All(),
                    AutoId = _id
                });
            }
        }

        private bool Load()
        {
            using (var sr = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var container = _serializer.Deserialize<DataContainer<TEntity>>(reader);

                if (container == null) return false;

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