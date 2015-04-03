using System;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;

namespace HydraTest.CommandHandlers
{
    public class TestBase : IDisposable
    {
        public TestBase()
        {
            Context = ShimsContext.Create();
            Transaction = new ShimSMTPTransaction();
            Core = new ShimSMTPCore();

            ShimSMTPTransaction.BehaveAsNotImplemented();
            ShimSMTPCore.BehaveAsNotImplemented();
        }

        protected ShimSMTPCore Core { get; set; }

        protected ShimSMTPTransaction Transaction { get; set; }

        private IDisposable Context { get; set; }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
