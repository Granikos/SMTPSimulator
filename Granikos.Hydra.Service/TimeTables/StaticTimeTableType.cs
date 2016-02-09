using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.Hydra.Service.Models;
using log4net;

namespace Granikos.Hydra.Service.TimeTables
{
    [DisplayName("Static")]
    [Export("static", typeof (ITimeTableType))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StaticTimeTableType : ITimeTableType
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (StaticTimeTableType));
        private readonly IDictionary<string, string> _initialParameters = new Dictionary<string, string>();
        private bool[] _intervals;
        private int _numMails;
        public IDictionary<string, string> Parameters { get; set; }

        public IDictionary<string, string> Data
        {
            get { return null; }
        }

        public DateTime GetNextMailTime()
        {
            const int numIntervals = 7*24;

            var time = DateTime.Now;
            var offset = time.Hour + 24*(int) time.DayOfWeek;

            var initial = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Local);

            DateTime nextTime;
            if (_intervals[offset])
            {
                var perInterval = 3600000.0/_numMails;
                var diff = (time.Minute*60 + time.Second)*1000 + time.Millisecond;
                var newDiff = (int) (Math.Ceiling(diff/perInterval)*perInterval);

                nextTime = initial.AddMilliseconds(newDiff);

                Logger.DebugFormat("Next mail time is {0}", nextTime);

                return nextTime;
            }
            int i;
            for (i = 0; i <= numIntervals; i++)
            {
                var index = (offset + i)%numIntervals;
                if (_intervals[index]) break;
            }

            if (i > numIntervals)
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
            if (!Parameters.ContainsKey("staticMailsPerInterval"))
            {
                message = "Missing staticMailsPerInterval.";
                return false;
            }
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

            if (!Parameters.ContainsKey("staticData"))
            {
                message = "Missing staticData.";
                return false;
            }
            var values = Parameters["staticData"].Split(',');
            if (values.Length != 24*7*4)
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
                        message = "Invalid interval data value for stgatic time table: '" + values[i] + "'";
                        return false;
                }
            }

            message = null;
            return true;
        }

        public void Initialize()
        {
            _numMails = int.Parse(Parameters["staticMailsPerInterval"]);
            _intervals = Parameters["staticData"]
                .Split(',')
                .Select(x => x.Equals("1"))
                .ToArray();
        }

        public ReadOnlyDictionary<string, string> InitialParameters
        {
            get
            {
                if (_initialParameters.Count == 0)
                {
                    var intervals = string.Join(",", Enumerable.Range(0, 24*7*4).Select(_ => "0"));

                    _initialParameters.Add("staticData", intervals);
                    _initialParameters.Add("staticMailsPerInterval", "1");
                }

                return new ReadOnlyDictionary<string, string>(_initialParameters);
            }
        }
    }
}