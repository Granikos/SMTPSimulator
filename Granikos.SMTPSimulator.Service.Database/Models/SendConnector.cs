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
using System.Net.Security;
using System.Security.Authentication;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public class SendConnector : ISendConnector
    {
        private TLSSettings _tlsSettings;
        private int _retryTimeInternal;
        private ICollection<Domain> _internalDomains = new List<Domain>();

        public SendConnector()
            : this(null)
        {
        }

        public SendConnector(IPAddress remoteIP)
        {
            RemoteAddress = remoteIP;
            UseSmarthost = RemoteAddress != null;
            LocalAddress = IPAddress.Any;
            RemotePort = 25;
            TLSSettings = new TLSSettings();
        }

        public bool Default { get; set; }

        [Required]
        public string Name { get; set; }

        [NotMapped]
        public IPAddress LocalAddress { get; set; }

        [Required]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string LocalAddressString
        {
            get { return LocalAddress.ToString(); }
            set { LocalAddress = IPAddress.Parse(value); }
        }

        [NotMapped]
        public IPAddress RemoteAddress { get; set; }

        [RegularExpression(
            @"^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))?$")]
        public string RemoteAddressString
        {
            get { return RemoteAddress != null ? RemoteAddress.ToString() : null; }
            set { RemoteAddress = value != null ? IPAddress.Parse(value) : null; }
        }

        [Required]
        [Range(0, 65535)]
        public int RemotePort { get; set; }

        public bool UseSmarthost { get; set; }

        [Required]
        public TLSSettings TLSSettings
        {
            get { return _tlsSettings; }
            set { _tlsSettings = value ?? new TLSSettings(); }
        }

        public int RetryTimeInternal
        {
            get { return _retryTimeInternal; }
            set
            {
                if (!(value > 0)) throw new ArgumentException();
                _retryTimeInternal = value;
            }
        }

        [NotMapped]
        public TimeSpan RetryTime
        {
            get { return TimeSpan.FromSeconds(RetryTimeInternal); }
            set { RetryTimeInternal = (int)value.TotalSeconds; }
        }

        [Range(0, 10)]
        public int RetryCount { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        public bool UseAuth { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string[] Domains
        {
            get
            {
                return InternalDomains.Select(d => d.DomainName).ToArray();
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                InternalDomains = value.Select(d => new Domain { DomainName = d}).ToList();
            }
        }

        public virtual ICollection<Domain> InternalDomains
        {
            get { return _internalDomains; }
            set { _internalDomains = (value ?? new List<Domain>()); }
        }

        public bool RequireTLS
        {
            get { return TLSSettings.Mode == TLSMode.Required; }
        }

        public bool EnableTLS
        {
            get { return TLSSettings.Mode != TLSMode.Disabled && TLSSettings.Mode != TLSMode.FullTunnel; }
        }

        public bool TLSFullTunnel
        {
            get { return TLSSettings.Mode == TLSMode.FullTunnel; }
        }

        public ICredentials Credentials
        {
            get { return Username != null ? new NetworkCredential(Username, Password) : null; }
        }

        public EncryptionPolicy TLSEncryptionPolicy
        {
            get { return TLSSettings.EncryptionPolicy; }
        }

        public SslProtocols SslProtocols
        {
            get { return TLSSettings.SslProtocols; }
        }

        public bool ValidateCertificateRevocation
        {
            get { return TLSSettings.ValidateCertificateRevocation; }
        }
    }

    public class Domain
    {
        [Key]
        public int Id { get; set; }

        public int ConnectorId { get; set; }

        [ForeignKey("ConnectorId")]
        public virtual SendConnector Connector { get; set; }

        [Required]
        public string DomainName { get; set; }
    }
}