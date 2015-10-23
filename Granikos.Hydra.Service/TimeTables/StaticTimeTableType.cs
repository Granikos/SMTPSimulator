using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Providers;

namespace Granikos.Hydra.Service.TimeTables
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
            if (Parameters.ContainsKey("staticMailsPerInterval"))
            {
                int value;
                if (!int.TryParse(Parameters["staticMailsPerInterval"], out value))
                {
                    message = "Invalid value for staticMailsPerInterval, not a valid integer.";
                    return false;
                }
                if (value <= 0 || value > 100)
                {
                    message = "Invalid value for staticMailsPerInterval, value out of range 1-100.";
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