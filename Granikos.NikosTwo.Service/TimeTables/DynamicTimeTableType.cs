using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.NikosTwo.Service.Models;
using log4net;

namespace Granikos.NikosTwo.Service.TimeTables
{
    [DisplayName("Dynamic")]
    [Export("dynamic", typeof(ITimeTableType))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DynamicTimeTableType : ITimeTableType
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DynamicTimeTableType));
        private readonly IDictionary<string, string> _initialParameters = new Dictionary<string, string>();
        private double[] _values;
        private int _totalMails;

        public IDictionary<string, string> Parameters { get; set; }

        public IDictionary<string, string> Data
        {
            get
            {
                return null; // TODO: Maybe Presets?
            }
        }

        public DateTime GetNextMailTime()
        {
            var time = DateTime.Now;
            var initial = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Local);

            DateTime nextTime;
            if (_values[time.Hour] > 0)
            {
                var mailCount = Math.Round(_values[time.Hour]*_totalMails);
                var perInterval = 3600000.0 / mailCount;
                var diff = (time.Minute * 60 + time.Second) * 1000 + time.Millisecond;
                var newDiff = (int)(Math.Ceiling(diff / perInterval) * perInterval);

                nextTime = initial.AddMilliseconds(newDiff);

                Logger.DebugFormat("Next mail time is {0}", nextTime);

                return nextTime;
            }
            int i;
            for (i = 0; i <= 24; i++)
            {
                var index = (time.Hour + i) % 24;
                if (_values[index] > 0) break;
            }

            if (i > 24)
            {
                Logger.Debug("No interval is active, so no next mail");
                return DateTime.MaxValue;
            }
            nextTime = initial.AddHours(i);

            Logger.DebugFormat("Next mail time is {0}", nextTime);

            return nextTime;
        }

        public bool ValidateParameters(out string message)
        {
            if (!Parameters.ContainsKey("dynamicTotalMails"))
            {
                message = "Missing dynamicTotalMails.";
                return false;
            }
            int value;
            if (!int.TryParse(Parameters["dynamicTotalMails"], out value))
            {
                message = "Invalid value for dynamicTotalMails, not a valid integer.";
                return false;
            }
            if (value <= 0)
            {
                message = "Invalid value for dynamicTotalMails, must be positive.";
                return false;
            }

            if (!Parameters.ContainsKey("dynamicData"))
            {
                message = "Missing dynamicData.";
                return false;
            }
            var values = Parameters["dynamicData"].Split(',');
            if (values.Length != 24)
            {
                message = "Invalid number of dynamic time table values.";
                return false;
            }

            double sum = 0;
            for (var i = 0; i < 24; i++)
            {
                double dataValue;
                if (!Double.TryParse(values[i], out dataValue))
                {
                    message = "Invalid interval data value for dynamic time table: '" + values[i] + "'";
                    return false;
                }

                if (dataValue < 0 || dataValue > 1)
                {
                    message = "Invalid interval data value for dynamic time table: '" + values[i] + "'";
                    return false;
                }

                sum += dataValue;
            }

            if (Math.Abs(sum - 1) > 0.001)
            {
                message = "Fractions for dynamic time table do not add up to 1.";
                return false;
            }

            message = null;
            return true;
        }

        public void Initialize()
        {
            _totalMails = int.Parse(Parameters["dynamicTotalMails"]);
            _values = Parameters["dynamicData"]
                .Split(',')
                .Select(Double.Parse)
                .ToArray();
        }

        public ReadOnlyDictionary<string, string> InitialParameters
        {
            get
            {
                if (_initialParameters.Count == 0)
                {
                    var intervals = string.Join(",", Enumerable.Range(0, 24).Select(i => i >= 8 && i <= 16? (1.0/9) : 0));

                    _initialParameters.Add("dynamicData", intervals);
                    _initialParameters.Add("dynamicTotalMails", "90");
                }

                return new ReadOnlyDictionary<string, string>(_initialParameters);
            }
        }
    }
}