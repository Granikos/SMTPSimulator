﻿using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace HydraService
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
            new ServiceController(serviceInstaller1.ServiceName).Start();
        }
    }
}