using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Providers;

namespace Granikos.Hydra.Service.TimeTables
{
    [DisplayName("Dynamic")]
    [Export("dynamic", typeof(ITimeTableType))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DynamicTimeTableType : ITimeTableType
    {
        public IDictionary<string, string> Parameters { get; set; }

        public IDictionary<string, string> Data
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"TODO", "TODO"}
                };
            }
        }

        public DateTime GetNextMailTime()
        {
            throw new NotImplementedException();
        }

        public bool ValidateParameters(out string message)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public ReadOnlyDictionary<string, string> InitialParameters
        {
            get
            {
                return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            }
        }
    }
}