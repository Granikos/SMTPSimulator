using System;
using System.Net;
using System.ServiceProcess;
using HydraCore;

namespace HydraService
{
    public partial class SMTPService : ServiceBase
    {
        private SMTPServer _server;
        private SMTPCore _core;

        public SMTPService()
        {
            InitializeComponent();
            var ip = IPAddress.Parse("127.0.0.1");
            var port = 1337;

            _core = new SMTPCore();
            _core.AddSubnet(new IPSubnet("127.0.0.1/24"));
            _core.Greet = "Test Greet";
            _core.ServerName = "localhost";
            _core.Banner = "This is the banner text!";

            _server = new SMTPServer(new IPEndPoint(ip, port), _core);
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
