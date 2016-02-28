using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Granikos.NikosTwo.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                new ServiceController(serviceInstaller1.ServiceName).Start();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}