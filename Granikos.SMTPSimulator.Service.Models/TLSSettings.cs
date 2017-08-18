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
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Granikos.SMTPSimulator.Service.Models
{
    [ComplexType]
    [DataContract]
    public class TLSSettings
    {
        public TLSSettings()
        {
            Mode = TLSMode.Enabled;
            SslProtocols = SslProtocols.Default;
            EncryptionPolicy = EncryptionPolicy.RequireEncryption;
            AuthLevel = TLSAuthLevel.EncryptionOnly;
        }

        [DataMember]
        public TLSMode Mode { get; set; }

        [DataMember]
        public SslProtocols SslProtocols { get; set; }

        [DataMember]
        public EncryptionPolicy EncryptionPolicy { get; set; }

        [DataMember]
        public TLSAuthLevel AuthLevel { get; set; }

        [DataMember]
        public bool ValidateCertificateRevocation { get; set; }

        [DataMember]
        public string CertificateName { get; set; }

        [DataMember]
        public string CertificatePassword { get; set; }

        [DataMember]
        public string CertificateDomain { get; set; }

        [DataMember]
        public string CertificateType { get; set; }
    }

    public enum TLSAuthLevel
    {
        EncryptionOnly,
        CertificateValidation,
        DomainValidation
    }
    public enum TLSMode
    {
        Disabled,
        Enabled,
        Required,
        FullTunnel
    }
}