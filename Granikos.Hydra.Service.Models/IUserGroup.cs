namespace Granikos.NikosTwo.Service.Models
{
    public interface IUserGroup : IEntity<int>
    {
        string Name { get; set; }

        int[] UserIds { get; set; }
    }
}