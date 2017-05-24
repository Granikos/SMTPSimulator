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
using System.Net;
using log4net;

namespace Granikos.SMTPSimulator.Service
{
    // TODO: Clean up grey list regularly
    internal class GreylistingManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (GreylistingManager));

        private readonly Dictionary<IPAddress, GreylistTimeWindow> _greyList =
            new Dictionary<IPAddress, GreylistTimeWindow>();

        private TimeSpan _greylistTime;
        private TimeSpan _greylistWindow;

        public GreylistingManager(TimeSpan greylistTime, TimeSpan? greylistWindow = null)
        {
            if (greylistTime < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("The greylisting time must be positive");
            if (greylistWindow != null && greylistWindow.Value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("The greylisting window must be greater than zero");

            _greylistTime = greylistTime;
            _greylistWindow = greylistWindow ?? TimeSpan.FromMinutes(15);
        }

        public TimeSpan GreylistWindow
        {
            get { return _greylistWindow; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The greylisting window must be greater than zero");
                _greylistWindow = value;
            }
        }

        public TimeSpan GreylistTime
        {
            get { return _greylistTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The greylisting time must be positive");
                _greylistTime = value;
            }
        }

        public bool Enabled
        {
            get { return GreylistTime > TimeSpan.Zero; }
        }

        public bool IsGreylisted(IPAddress ip)
        {
            if (!Enabled) return false;

            GreylistTimeWindow window;

            if (_greyList.TryGetValue(ip, out window))
            {
                if (window.Start > DateTime.Now)
                {
                    Logger.InfoFormat("Greylisting activate for {0}, time left: {1}", ip, (window.Start - DateTime.Now));
                    return true;
                }

                if (window.End > DateTime.Now)
                {
                    return false;
                }

                _greyList.Remove(ip);
            }

            var start = (DateTime.Now + GreylistTime);

            Logger.InfoFormat("Greylisting started for {0}, time left: {1}", ip, GreylistTime);
            _greyList.Add(ip, new GreylistTimeWindow {Start = start, End = start + GreylistWindow});

            return true;
        }

        private struct GreylistTimeWindow
        {
            public DateTime End;
            public DateTime Start;
        }
    }
}