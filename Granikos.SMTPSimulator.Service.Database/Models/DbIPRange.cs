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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public class DbIPRange : IIpRange
    {
        [Key]
        public int Id { get; set; }

        public ReceiveConnector Connector { get; set; }
        private IPAddress _start;
        private IPAddress _end;

        protected DbIPRange()
        {
            
        }

        public DbIPRange(IPAddress start, IPAddress end)
        {
            if (start == null) throw new ArgumentNullException();
            if (end == null) throw new ArgumentNullException();
            if (!(start.AddressFamily == end.AddressFamily)) throw new ArgumentException();

            Start = start;
            End = end;
        }

        [NotMapped]
        public IPAddress Start
        {
            get { return _start; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(End == null || value.AddressFamily == End.AddressFamily)) throw new ArgumentException();

                _start = value;
            }
        }

        [NotMapped]
        public IPAddress End
        {
            get { return _end; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(Start == null || value.AddressFamily == Start.AddressFamily)) throw new ArgumentException();

                _end = value;
            }
        }

        public string StartString
        {
            get { return Start.ToString(); }
            set { Start = IPAddress.Parse(value); }
        }

        public string EndString
        {
            get { return End.ToString(); }
            set { End = IPAddress.Parse(value); }
        }

        public static DbIPRange FromOther(IIpRange range)
        {
            return new DbIPRange(range.Start, range.End);
        }
    }
}