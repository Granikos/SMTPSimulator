using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Granikos.NikosTwo.SmtpServer
{
    [Export(typeof (IModuleLoader<>))]
    public class DefaultModuleLoader<T> : IModuleLoader<T>
        where T : class
    {
        private readonly ComposablePartCatalog _catalog;
        private readonly string _nameAttribute;

        public DefaultModuleLoader(ComposablePartCatalog catalog, string nameAttribute)
        {
            _catalog = catalog;
            _nameAttribute = nameAttribute;
        }

        public IEnumerable<Tuple<string, T>> GetModules()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Tuple<string, T>>>() != null);

            var container = new CompositionContainer(_catalog);
            container.ComposeExportedValue(_catalog);

            var exports = container.GetExports<T, Dictionary<string, object>>().ToList();
            var modules =
                exports.Select(export => new Tuple<string, T>(export.Metadata[_nameAttribute].ToString(), export.Value));

            return modules;
        }
    }
}