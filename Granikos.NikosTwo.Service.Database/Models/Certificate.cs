using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.Database.Models
{
    public class Certificate : ICertificate
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(450)]
        public string Name { get; set; }

        [NotMapped]
        public byte[] Content
        {
            get { return InternalContent != null ? InternalContent.Content : null; }
            set { InternalContent = new CertificateContent { Content = value }; }
        }

        public CertificateContent InternalContent { get; set; }
    }

    public class CertificateContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength]
        public byte[] Content { get; set; }
    }
}