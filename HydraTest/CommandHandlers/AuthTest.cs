using System;
using System.Collections.Generic;
using HydraCore.AuthMethods;
using HydraCore.AuthMethods.Fakes;
using HydraCore.CommandHandlers;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace HydraTest.CommandHandlers
{
    public class AUTHTest : TestBase
    {
        [Fact]
        public void TestInitialize()
        {
            using (ShimsContext.Create())
            {
                var ehloLines = new List<string>();
                AddCoreListProperty("EHLOLines", () => ehloLines);

                var handler = new AUTHHandler();
                handler.Loader = new StubIAuthMethodLoader
                {
                    GetModules = () => new List<Tuple<string, IAuthMethod>>
                    {
                        new Tuple<string, IAuthMethod>("Test", new StubIAuthMethod()),
                        new Tuple<string, IAuthMethod>("Something", new StubIAuthMethod()),
                    }
                };
                handler.Initialize(Core);

                Assert.Contains("AUTH TEST SOMETHING", ehloLines);
            }
        }
    }
}
