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
using System.IO;
using System.Reflection;
using log4net.Core;
using log4net.Layout;

namespace Granikos.SMTPSimulator.Core.Logging
{
    public class CsvPatternLayout : PatternLayout
    {
        public override string Header
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return base.Header.Replace("%date", DateTime.Now.ToString("o"))
                    .Replace("%version", version.ToString());
            }

            set { base.Header = value; }
        }

        public override void ActivateOptions()
        {
            // register custom pattern tokens
            AddConverter("newfield", typeof (NewFieldConverter));
            AddConverter("endrow", typeof (EndRowConverter));
            base.ActivateOptions();
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent.MessageObject != null)
            {
                var properties = loggingEvent.MessageObject.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(loggingEvent.MessageObject, null);
                    loggingEvent.Properties[prop.Name] = value;
                }
            }

            var ctw = new CsvTextWriter(writer);
            // write the starting quote for the first field
            ctw.WriteQuote();
            base.Format(ctw, loggingEvent);
        }
    }
}