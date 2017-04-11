using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public class Attachment : IAttachment
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(450)]
        public string Name { get; set; }

        [Range(0, Int32.MaxValue)]
        public int Size { get; set; }

        [NotMapped]
        public byte[] Content
        {
            get { return InternalContent != null ? InternalContent.Content : null; }
            set { InternalContent = new AttachmentContent {Content = value}; }
        }

        public AttachmentContent InternalContent { get; set; }
    }

    public class AttachmentContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength]
        public byte[] Content { get; set; }
    }
}
