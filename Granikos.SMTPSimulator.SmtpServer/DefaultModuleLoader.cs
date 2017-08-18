// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Granikos.SMTPSimulator.SmtpServer
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
            var container = new CompositionContainer(_catalog);
            container.ComposeExportedValue(_catalog);

            var exports = container.GetExports<T, Dictionary<string, object>>().ToList();
            var modules =
                exports.Select(export => new Tuple<string, T>(export.Metadata[_nameAttribute].ToString(), export.Value));

            return modules;
        }
    }
}