using Granikos.NikosTwo.Service.ConfigurationService.Models;
using Granikos.NikosTwo.Service.Models;

namespace Granikos.NikosTwo.Service
{
    public interface IMailQueueProvider
    {
        void Enqueue(MailMessage mail);
    }
}