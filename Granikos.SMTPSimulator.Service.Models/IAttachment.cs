namespace Granikos.SMTPSimulator.Service.Models
{
    public interface IAttachment
    {
        string Name { get; set; }

        int Size { get; set; }

        byte[] Content { get; set; }
    }
}
