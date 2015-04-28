using System;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using HydraCore;
using HydraCore.CommandHandlers;

namespace HydraService
{
    public partial class SMTPService : ServiceBase
    {
        private readonly SMTPServer _server;
        private readonly SMTPServer _server2;

        private ServiceHost _host;
        private SMTPCore core;

        public SMTPService()
        {
            InitializeComponent();
            var ip = IPAddress.Parse("127.0.0.1");
            var port = 1337;

            using (var catalog = new AssemblyCatalog(typeof (SMTPCore).Assembly)) // TODO: Use Dependency Injection
            {
                var loader = new CommandHandlerLoader(catalog);
                core = new SMTPCore(loader);
            }

            core.Config = new ServerConfig
            {
                Greet = "Test Greet",
                ServerName = "localhost",
                Banner = "This is the banner text!"
            };

            _server = new SMTPServer(new IPEndPoint(ip, port), core);
            _server.AddSubnet(new IPSubnet("127.0.0.1/24"));

            _server2 = new SMTPServer(new IPEndPoint(ip, 1338), core);
            _server2.UseSsl = true;
            _server2.AddSubnet(new IPSubnet("127.0.0.1/24"));
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
            _server2.Start();

            if (_host != null)
            {
                _host.Close();
            }

            // http://localhost:1339/service.svc
            // _host = new ServiceHost(typeof(ConfigurationService));
            _host = new ServiceHost(new ConfigurationService(core));

            _host.Open();
        }

        protected override void OnStop()
        {
            _server.Stop();
            _server2.Start();

            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }
    }
}