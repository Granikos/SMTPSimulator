using System;
using System.ServiceProcess;
using log4net.Config;

namespace HydraService
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            if (Environment.UserInteractive)
            {
                var service = new SMTPService();
                service.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new SMTPService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}