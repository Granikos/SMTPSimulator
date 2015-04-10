using HydraCore.CommandHandlers;

namespace HydraCore.CommandHandlers
{
    public interface ICommandHandlerLoader : IModuleLoader<ICommandHandler>
    {
        
    }
}