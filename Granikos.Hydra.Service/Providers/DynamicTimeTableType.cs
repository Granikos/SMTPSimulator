using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Granikos.Hydra.Service.Providers
{
    [DisplayName("Dynamic")]
    [Export("dynamic", typeof(ITimeTableType))]
    public class DynamicTimeTableType : ITimeTableType
    {
        public string DisplayName { get; private set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string[] Data { get; set; }
    }
}