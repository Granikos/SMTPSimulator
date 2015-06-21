using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraService.Providers;

namespace HydraService
{
    public partial class SMTPService : ServiceBase, ISMTPServerContainer
    {
        private readonly CompositionContainer _container;
        private readonly SMTPCore _core;
        private ServiceHost _host;

        [Import]
        private IRecieveConnectorProvider _recieveConnectors;

        private SMTPServer[] _servers;

        public bool Running { get; private set; }

        public SMTPService()
        {
            InitializeComponent();

            var pluginFolder = ConfigurationManager.AppSettings["PluginFolder"];

            ComposablePartCatalog catalog;

            if (!string.IsNullOrEmpty(pluginFolder))
            {
                catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof (SMTPService).Assembly),
                    new DirectoryCatalog(AssemblyDirectory),
                    new DirectoryCatalog(pluginFolder)
                    );
            }
            else
            {
                catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof (SMTPService).Assembly),
                    new DirectoryCatalog(AssemblyDirectory)
                    );
            }

            _container = new CompositionContainer(catalog);
            _container.SatisfyImportsOnce(this);

            var loader = new CommandHandlerLoader(catalog);
            _core = new SMTPCore(loader);

            RefreshServers();
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal void RefreshServers()
        {
            StopSMTPServers();

            _servers = _recieveConnectors.All().Select(r =>
            {
                var server = new SMTPServer(r, _core, _container);
                _container.SatisfyImportsOnce(server);
                return server;
            })
                .ToArray();

            StartSMTPServers();
        }

        public void StopSMTPServers()
        {
            if (Running && _servers != null)
            {
                foreach (var server in _servers)
                {
                    server.Stop();
                }
            }

            Running = false;
        }

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            StartSMTPServers();

            if (_host != null)
            {
                _host.Close();
            }

            var service = new ConfigurationService(_core, this);
            _container.ComposeParts(service);

            _host = new ServiceHost(service);

            _host.Open();
        }

        public void StartSMTPServers()
        {
            if (!Running && _servers != null)
            {
                foreach (var server in _servers)
                {
                    server.Start();
                }
            }

            Running = true;
        }

        protected override void OnStop()
        {
            StopSMTPServers();

            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }
    }
}