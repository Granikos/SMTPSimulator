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
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.ConfigurationService.Models
{
    [DataContract]
    public class SendConnector : ISendConnector
    {
        [Required]
        [DataMember]
        [RegularExpression(
            @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
        public string LocalAddressString
        {
            get { return LocalAddress.ToString(); }
            set { LocalAddress = IPAddress.Parse(value); }
        }

        [DataMember]
        [RegularExpression(
            @"^((?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))?$")]
        public string RemoteAddressString
        {
            get { return RemoteAddress != null ? RemoteAddress.ToString() : null; }
            set { RemoteAddress = value != null ? IPAddress.Parse(value) : null; }
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public IPAddress LocalAddress { get; set; }
        public IPAddress RemoteAddress { get; set; }

        [Required]
        [DataMember]
        [Range(0, 65535)]
        public int RemotePort { get; set; }

        [DataMember]
        public bool UseSmarthost { get; set; }

        [DataMember]
        public TLSSettings TLSSettings { get; set; }

        [DataMember]
        public TimeSpan RetryTime { get; set; }

        [DataMember]
        public int RetryCount { get; set; }

        [DataMember]
        public string[] Domains { get; set; }

        [DataMember]
        public bool UseAuth { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

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
}