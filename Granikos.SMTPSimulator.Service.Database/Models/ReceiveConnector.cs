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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public class ReceiveConnector : IReceiveConnector
    {
        private ICollection<DbIPRange> _remoteIPRanges = new List<DbIPRange>();
        private TLSSettings _tlsSettings;

        public ReceiveConnector()
        {
            Address = IPAddress.Any;
            Port = 25;
            _tlsSettings = new TLSSettings();
        }

        public bool Enabled { get; set; }

        [Required]
        public string Name { get; set; }

        [NotMapped]
        public IPAddress Address { get; set; }

        [Required]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string AddressString
        {
            get { return Address.ToString(); }
            set { Address = IPAddress.Parse(value); }
        }

        [Required]
        [Range(0, 65535)]
        public int Port { get; set; }

        [NotMapped]
        IIpRange[] IReceiveConnector.RemoteIPRanges
        {
            get { return RemoteIPRanges.Select(r => (IIpRange)r).ToArray(); }
            set { RemoteIPRanges = value.Select(DbIPRange.FromOther).ToArray(); }
        }

        public virtual ICollection<DbIPRange> RemoteIPRanges
        {
            get { return _remoteIPRanges; }
            set { _remoteIPRanges = (value ?? new List<DbIPRange>()); }
        }

        [Required]
        public string Banner { get; set; }

        public bool RequireAuth { get; set; }

        public string AuthUsername { get; set; }

        public string AuthPassword { get; set; }

        [Required]
        public virtual TLSSettings TLSSettings
        {
            get { return _tlsSettings; }
            set { _tlsSettings = value ?? new TLSSettings(); }
        }

        [Range(0, Int32.MaxValue)]
        public int? GreylistingTimeInternal {get; set; }

        [NotMapped]
        // TODO: [Range(TimeSpan.Zero, TimeSpan.MaxValue)]
        public TimeSpan? GreylistingTime
        {
            get { return GreylistingTimeInternal != null? TimeSpan.FromSeconds(GreylistingTimeInternal.Value) : (TimeSpan?) null; }
            set { GreylistingTimeInternal = value != null? (int)value.Value.TotalSeconds : (int?) null; }
        }

        [Required]
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        public static ReceiveConnector FromOther(IReceiveConnector source)
        {
            var target = new ReceiveConnector();

            source.CopyTo(target);

            return target;
        }
    }
}