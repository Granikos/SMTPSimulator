using System.Collections.Generic;

namespace Granikos.Hydra.Service.Models.Providers
{
    public interface IAttachmentProvider<TAttachment>
        where TAttachment :  IAttachment
    {
        int Total { get; }

        IEnumerable<TAttachment> All();

        TAttachment Get(string name);

        bool Add(TAttachment entity);

        bool Rename(string oldName, string newName);

        bool Delete(string name);

        bool Clear();
    }

    public interface IAttachmentProvider : IAttachmentProvider<IAttachment>
    {
    }
}