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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [DisplayName("File")]
    [Export(typeof(ICertificateFileProvider))]
    [Export("file", typeof(ICertificateProvider))]
    public class CertificateFileProvider : ICertificateFileProvider<Certificate>, ICertificateFileProvider, ICertificateProvider
    {
        [Import]
        protected ServiceDbContext Database;

        public int Total { get { return Database.Certificates.Count(); } }

        public IEnumerable<Certificate> All()
        {
            return Database.Certificates
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToList();
        }

        ICertificate ICertificateFileProvider<ICertificate>.Get(string name)
        {
            return Get(name);
        }

        public bool Add(ICertificate entity)
        {
            var cert = new Certificate
            {
                Name = entity.Name,
                Content = entity.Content
            };

            return Add(cert);
        }

        IEnumerable<ICertificate> ICertificateFileProvider<ICertificate>.All()
        {
            return All();
        }

        public Certificate Get(string name)
        {
            return Database.Certificates
                .Include("InternalContent")
                .Single(a => a.Name == name);
        }

        public Certificate GetInternal(string name)
        {
            return Database.Certificates
                .Single(a => a.Name == name);
        }

        public bool Add(Certificate entity)
        {
            var content = new CertificateContent { Content = entity.Content };

            if (Database.Certificates.Any(c => c.Name.Equals(entity.Name)))
            {
                return false;
            }

            entity.InternalContent = content;

            Database.CertificateContents.Add(content);
            Database.Certificates.Add(entity);

            Database.SaveChanges();

            return true;
        }

        public bool Delete(string name)
        {
            var at = GetInternal(name);
            Database.Certificates.Remove(at);
            Database.SaveChanges();

            return true;
        }

        public bool Clear()
        {
            throw new NotImplementedException();
        }

        public X509Certificate2 GetCertificate(string name, string password)
        {
            return new X509Certificate2(GetInternal(name).Content, password);
        }

        public IEnumerable<string> ListCertificates()
        {
            return Database.Certificates.Select(c => c.Name).ToArray();
        }
    }
}