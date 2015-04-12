using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace HydraCore.CommandHandlers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandHandlerAttribute : ExportAttribute
    {
        [ExcludeFromCodeCoverage]
        public CommandHandlerAttribute() : base(typeof (ICommandHandler))
        {
        }

        public string Command { get; set; }
    }
}