using System;
using System.ComponentModel.Composition;

namespace HydraCore.CommandHandlers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandHandlerAttribute : ExportAttribute
    {
        public CommandHandlerAttribute() : base(typeof(ICommandHandler)) { }

        public string Command { get; set; }
    }
}