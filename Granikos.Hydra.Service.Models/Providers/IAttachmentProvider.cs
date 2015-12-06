namespace Granikos.Hydra.Service.Models.Providers
{
    public interface IAttachmentProvider
    {

        int[] GetAttachmentIds();

        byte[] GetAttachmentContent(int id);
    }
}