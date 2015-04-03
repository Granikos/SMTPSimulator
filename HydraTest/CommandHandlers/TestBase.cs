using System;
using HydraCore.Fakes;
using Microsoft.QualityTools.Testing.Fakes;

namespace HydraTest.CommandHandlers
{
    public class TestBase : IDisposable
    {
        public TestBase()
        {
            _context = ShimsContext.Create();
            Transaction = new ShimSMTPTransaction();
            Core = new ShimSMTPCore();

            Transaction.ServerGet = () => Core;

            ShimSMTPTransaction.BehaveAsNotImplemented();
            ShimSMTPCore.BehaveAsNotImplemented();
        }

        protected ShimSMTPCore Core { get; set; }

        protected ShimSMTPTransaction Transaction { get; set; }

        private readonly IDisposable _context;

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
