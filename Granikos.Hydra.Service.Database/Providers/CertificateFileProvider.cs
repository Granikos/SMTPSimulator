using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
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