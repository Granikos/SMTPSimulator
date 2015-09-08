using HydraService.Models;

namespace HydraService
{
    public interface IMailQueueProvider
    {
        void Enqueue(MailMessage mail);
    }
}