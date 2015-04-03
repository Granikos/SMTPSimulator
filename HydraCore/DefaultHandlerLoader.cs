using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using HydraCore.CommandHandlers;

namespace HydraCore
{
    public interface IHandlerLoader
    {
        IEnumerable<Tuple<string, ICommandHandler>> GetHandlers();
    }

    public class DefaultHandlerLoader : IHandlerLoader
    {
        private readonly ComposablePartCatalog _catalog;

        public DefaultHandlerLoader(ComposablePartCatalog catalog)
        {
            _catalog = catalog;
        }

        public IEnumerable<Tuple<string, ICommandHandler>> GetHandlers()
        {
            foreach (var partDefinition in _catalog.Parts)
            {
                var part = partDefinition.CreatePart();
                foreach (var exportDefinition in partDefinition.ExportDefinitions)
                {
                    var handler = part.GetExportedValue(exportDefinition) as ICommandHandler;

                    if (handler != null)
                    {
                        var command = exportDefinition.Metadata["Command"].ToString();

                        yield return new Tuple<string, ICommandHandler>(command, handler);
                    }
                }
            }
        }
    }
}