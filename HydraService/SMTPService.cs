using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using HydraCore;
using HydraCore.CommandHandlers;
using HydraService.PriorityQueue;
using HydraService.Providers;
using MailMessage = HydraService.Models.MailMessage;

namespace HydraService
{
    public partial class SMTPService : ServiceBase, ISMTPServerContainer, IMailQueueProvider
    {
        // TODO: Use locks

        private readonly CompositionContainer _container;
        private readonly SMTPCore _core;
        private ServiceHost _host;

        [Import]
        private IRecieveConnectorProvider _recieveConnectors;

        [Import]
        private ISendConnectorProvider _sendConnectors;

        private SMTPServer[] _servers;

        private MessageSender[] _senders;

        private readonly DelayedQueue<Mail> _mailQueue = new DelayedQueue<Mail>(1000);

        private const int NumSenders = 4;

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
            RefreshSenders();
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

        public bool Running { get; private set; }

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

        public void StopMessageProcessing()
        {
            if (_senders != null)
            {
                foreach (var sender in _senders)
                {
                    sender.Stop();
                }
            }
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

        public void StartMessageProcessing()
        {
            if (_senders != null)
            {
                foreach (var sender in _senders)
                {
                    sender.Start();
                }
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

        internal void RefreshSenders()
        {
            StopMessageProcessing();

            _senders = Enumerable.Range(0, NumSenders)
                .Select(r => new MessageSender(_container, _mailQueue))
                .ToArray();

            StartMessageProcessing();
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
            StartMessageProcessing();

            if (_host != null)
            {
                _host.Close();
            }

            var service = new ConfigurationService(_core, this, this);
            _container.ComposeParts(service);

            _host = new ServiceHost(service);

            _host.Open();
        }

        protected override void OnStop()
        {
            StopSMTPServers();
            StopMessageProcessing();

            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }

        public void Enqueue(MailMessage mail)
        {
            var from = new MailAddress(mail.Sender);
            var to = mail.Recipients.Select(r => new MailAddress(r)).ToArray();
            var parsed = new Mail(from, to, mail.Content);

            // TODO
            parsed.Settings = new DefaultSendSettings(_sendConnectors.DefaultConnector);

            _mailQueue.Enqueue(parsed, TimeSpan.Zero);
        }
    }
}