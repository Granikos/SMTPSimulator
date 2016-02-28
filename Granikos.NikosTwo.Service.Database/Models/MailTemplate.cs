using System.ComponentModel.DataAnnotations;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service.Database.Models
{
    public class MailTemplate : IMailTemplate
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public EncodingType HeaderEncoding { get; set; }
        public EncodingType SubjectEncoding { get; set; }
        public EncodingType BodyEncoding { get; set; }
        public string Html { get; set; }
        public string Text { get; set; }
        public MailTemplateTypeBehaviour Behaviour { get; set; }
    }
}