using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

namespace HydraCore.CommandHandlers
{
    [Export(typeof(ICommandHandlerLoader))]
    public class CommandHandlerLoader : DefaultModuleLoader<ICommandHandler>, ICommandHandlerLoader
    {
        [ImportingConstructor]
        public CommandHandlerLoader([Import] ComposablePartCatalog catalog)
            : base(catalog, "Command")
        {
        }
    }
}