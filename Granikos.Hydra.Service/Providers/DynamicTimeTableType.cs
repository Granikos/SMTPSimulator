using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Granikos.Hydra.Service.Providers
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
    }
}