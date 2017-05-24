// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.SMTPSimulator.Service.Models;
using log4net;

namespace Granikos.SMTPSimulator.Service.TimeTables
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
                        message = "Invalid interval data value for static time table: '" + values[i] + "'";
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