using System.Security.Cryptography.X509Certificates;

namespace Granikos.SMTPSimulator.Service.Models.Providers
{
    public interface IExternalMailboxGroupProvider<TUserGroup> : IDataProvider<TUserGroup, int>
        where TUserGroup : IUserGroup
    {
        TUserGroup Add(string name);
    }

    public interface IExternalMailboxGroupProvider : IExternalMailboxGroupProvider<IUserGroup>
    {
    }
}