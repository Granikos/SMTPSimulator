using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Granikos.NikosTwo.SmtpServer;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace NikosTwoTest
{
    public class DefaultHandlerLoaderTest
    {
        [Fact]
        public void Test()
        {
            using (ShimsContext.Create())
            {
                using (var cc = new TypeCatalog(typeof (TestHandler)))
                {
                    var loader = new DefaultModuleLoader<IModule>(cc, "Fu");
                    var modules = loader.GetModules().ToList();
                    Assert.Equal(1, modules.Count());

                    var module = modules.First();
                    Assert.Equal("Bar", module.Item1);
                    Assert.True(module.Item2 is TestHandler);
                }
            }
        }

        private interface IModule
        {
        }

        [ExportMetadata("Fu", "Bar")]
        [Export(typeof (IModule))]
        private class TestHandler : IModule
        {
        }
    }
}