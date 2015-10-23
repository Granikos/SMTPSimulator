using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Granikos.Hydra.Service.Providers
{
    [DisplayName("Static")]
    [Export("static", typeof(ITimeTableType))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StaticTimeTableType : ITimeTableType
    {
        public IDictionary<string, string> Parameters { get; set; }
        public IDictionary<string, string> Data { get { return null; } }
        public DateTime GetNextMailTime()
        {
            return DateTime.Now + TimeSpan.FromMinutes(1);
        }

        public bool ValidateParameters(out string message)
        {
            if (Parameters.ContainsKey("staticType"))
            {
                switch (Parameters["staticType"])
                {
                    case "1h":
                    case "15min":
                        break;
                    default:
                        message = "Invalid interval type for static time table.";
                        return false;
                }
            }
            if (Parameters.ContainsKey("staticData"))
            {
                var values = Parameters["staticData"].Split(',');
                if (values.Length != 24 * 7 * 4)
                {
                    message = "Invalid number of static time table intervals.";
                    return false;
                }

                for (var i = 0; i < 24*7*4; i++)
                {
                    switch (values[i])
                    {
                        case "1":
                        case "0":
                            break;
                        default:
                            message = "Invalid interval data value for stgatic time table: '"+values[i]+"'";
                            return false;
                    } 
                }
            }

            message = null;
            return true;
        }

        public void Initialize()
        {
        }
    }
}