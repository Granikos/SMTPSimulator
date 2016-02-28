namespace Granikos.NikosTwo.Service.Models
{
    public interface ICertificate
    {
        string Name { get; set; }

        byte[] Content { get; set; }
    }
}