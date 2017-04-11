using Granikos.SMTPSimulator.Service.ConfigurationService.Models;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service
{
    public interface IMailQueueProvider
    {
        void Enqueue(MailMessage mail);
    }
}