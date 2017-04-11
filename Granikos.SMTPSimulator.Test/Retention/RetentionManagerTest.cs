using System.Diagnostics;
using System.IO;
using Granikos.SMTPSimulator.Service.Retention;
using Microsoft.QualityTools.Testing.Fakes;
using Xunit;

namespace Granikos.SMTPSimulator.Test.Retention
{
    public class RetentionManagerTest
    {
        [Fact]
        public void TestHandlers()
        {
            var basedir =
                @"C:\Users\Manuel\Documents\Visual Studio 2013\Projects\NikosTwo\Granikos.SMTPSimulator.Service\bin\Debug\Logs\SystemLogs\Service";

            var manager = new RetentionWorker(basedir);
            
            manager.Run();

            // foreach (var dir in Directory.EnumerateDirectories(basedir, "*", SearchOption.AllDirectories)
            //using (ShimsContext.Create())
        }

    }
}