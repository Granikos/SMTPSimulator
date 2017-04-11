using System;
using System.ServiceProcess;
using log4net;

namespace Granikos.SMTPSimulator.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            var Logger = LogManager.GetLogger(typeof(Program));
            try
            {
                if (Environment.UserInteractive)
                {
                    var service = new NikosTwoService();
                    service.TestStartupAndStop(args);
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new NikosTwoService()
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