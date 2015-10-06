using System;
using System.ServiceProcess;
using log4net;
using log4net.Config;

namespace Granikos.Hydra.Service
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            try
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
            catch (Exception e)
            {
                Logger.Error("An error occured.", e);
            }
        }
    }
}