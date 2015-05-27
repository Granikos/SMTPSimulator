using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Net;
using System.Reflection;
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

        private ComposablePartCatalog _catalog;

        public SMTPService()
        {
            InitializeComponent();
            var ip = IPAddress.Parse("0.0.0.0");
            var port = 25;

            _catalog = new AggregateCatalog(new AssemblyCatalog(typeof (SMTPCore).Assembly),
                new AssemblyCatalog(typeof (ConfigurationService).Assembly)); // TODO: Use Dependency Injection

            using (var container = new CompositionContainer(_catalog))
            {
                var loader = new CommandHandlerLoader(_catalog);
                core = new SMTPCore(loader);

                core.Config = new ServerConfig
                {
                    Greet = "Test Greet",
                    ServerName = "localhost",
                    Banner = "This is the banner text!"
                };

                _server = new SMTPServer(new IPEndPoint(ip, port), core, container);
                container.SatisfyImportsOnce(_server);
            }
            // _server.AddSubnet(new IPSubnet("127.0.0.1/24"));

            /*
            _server2 = new SMTPServer(new IPEndPoint(ip, 1338), core);
            _server2.UseSsl = true;
            _server2.AddSubnet(new IPSubnet("127.0.0.1/24"));
             * */
        }

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            if (_server != null) _server.Start();
            if (_server2 != null) _server2.Start();

            if (_host != null)
            {
                _host.Close();
            }

            using (var container = new CompositionContainer(_catalog))
            {
                var service = new ConfigurationService(core);
                container.ComposeParts(service);

                // http://localhost:1339/service.svc
                // _host = new ServiceHost(typeof(ConfigurationService));
                _host = new ServiceHost(service);

                _host.Open();
            }

        }

        protected override void OnStop()
        {
            if (_server != null) _server.Stop();
            if (_server2 != null) _server2.Start();

            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }
    }
}