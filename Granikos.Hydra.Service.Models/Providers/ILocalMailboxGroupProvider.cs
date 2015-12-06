namespace Granikos.Hydra.Service.Models.Providers
{
    public interface ILocalMailboxGroupProvider<TUserGroup> : IDataProvider<TUserGroup, int>
        where TUserGroup : IUserGroup
    {
        TUserGroup Add(string name);
    }

    public interface ILocalMailboxGroupProvider : ILocalMailboxGroupProvider<IUserGroup>
    {
    }
}