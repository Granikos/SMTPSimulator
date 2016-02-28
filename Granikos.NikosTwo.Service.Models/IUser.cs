namespace Granikos.NikosTwo.Service.Models
{
    public interface IUser : IEntity<int>
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string Mailbox { get; set; }
    }
}