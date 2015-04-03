using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using HydraCore;
using HydraCore.CommandHandlers;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest
{
    public class DefaultHandlerLoaderTest
    {
        [ExportMetadata("Command", "Test")]
        [Export(typeof(ICommandHandler))]
        class TestHandler : ICommandHandler
        {
            public void Initialize(SMTPCore core)
            {
                throw new System.NotImplementedException();
            }

            public SMTPResponse Execute(SMTPTransaction transaction, string parameters)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public void Test()
        {
            using (ShimsContext.Create())
            {
                using (var cc = new TypeCatalog(typeof(TestHandler)))
                {
                    var loader = new DefaultHandlerLoader(cc);
                    var handlers = loader.GetHandlers().ToList();
                    Assert.Equal(1, handlers.Count());

                    var handler = handlers.First();
                    Assert.Equal("Test", handler.Item1);
                    Assert.True(handler.Item2 is TestHandler);
                    
                }

            }
        }
    }
}
