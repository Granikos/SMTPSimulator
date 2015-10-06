using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service
{
    public interface IMailQueueProvider
    {
        void Enqueue(MailMessage mail);
    }
}