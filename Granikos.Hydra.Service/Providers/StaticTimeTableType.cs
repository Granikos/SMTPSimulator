using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Granikos.Hydra.Service.Providers
{
    [DisplayName("Static")]
    [Export("static", typeof(ITimeTableType))]
    public class StaticTimeTableType : ITimeTableType
    {
        public string DisplayName { get; private set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string[] Data { get; set; }
    }
}