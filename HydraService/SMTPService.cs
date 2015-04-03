﻿using System;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.ServiceProcess;
using HydraCore;

namespace HydraService
{
    public partial class SMTPService : ServiceBase
    {
        private readonly SMTPServer _server;

        public SMTPService()
        {
            InitializeComponent();
            var ip = IPAddress.Parse("127.0.0.1");
            var port = 1337;

            SMTPCore core;
            using (var catalog = new AssemblyCatalog(typeof(SMTPCore).Assembly)) // TODO: Use Dependency Injection
            {
                var loader = new DefaultHandlerLoader(catalog);
                core = new SMTPCore(loader);
            }

            core.Greet = "Test Greet";
            core.ServerName = "localhost";
            core.Banner = "This is the banner text!";

            _server = new SMTPServer(new IPEndPoint(ip, port), core);
            _server.AddSubnet(new IPSubnet("127.0.0.1/24"));
        }

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            _server.Start();
        }

        protected override void OnStop()
        {
            _server.Stop();
        }
    }
}