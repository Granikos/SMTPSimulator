using System;
using System.Collections.Generic;
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
using Granikos.Hydra.Core;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.PriorityQueue;
using Granikos.Hydra.Service.Providers;
using Granikos.Hydra.Service.TimeTables;
using Granikos.Hydra.SmtpClient;
using Granikos.Hydra.SmtpServer;
using Granikos.Hydra.SmtpServer.CommandHandlers;
using MailMessage = Granikos.Hydra.Service.Models.MailMessage;

namespace Granikos.Hydra.Service
{
    public partial class HydraService : ServiceBase, ISMTPServerContainer, IMailQueueProvider
    {
        private const int NumSenders = 4;
        // TODO: Use locks

        private readonly CompositionContainer _container;
        private readonly DelayedQueue<SendableMail> _mailQueue = new DelayedQueue<SendableMail>(1000);
        private readonly SMTPServer _smtpServer;
        private ServiceHost _host;

        [Import]
        private IReceiveConnectorProvider _receiveConnectors;

        [Import]
        private ISendConnectorProvider _sendConnectors;

        [Import]
        private ITimeTableProvider _timeTables;

        private MessageSender[] _senders;
        private SMTPService[] _servers;
        private readonly Dictionary<int,TimeTableGenerator> _generators = new Dictionary<int, TimeTableGenerator>();

        public HydraService()
        {
            InitializeComponent();

            var pluginFolder = ConfigurationManager.AppSettings["PluginFolder"];

            ComposablePartCatalog catalog;

            if (!string.IsNullOrEmpty(pluginFolder))
            {
                catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof (HydraService).Assembly),
                    new DirectoryCatalog(AssemblyDirectory),
                    new DirectoryCatalog(pluginFolder)
                    );
            }
            else
            {
                catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof (HydraService).Assembly),
                    new DirectoryCatalog(AssemblyDirectory)
                    );
            }

            _container = new CompositionContainer(catalog);
            _container.ComposeExportedValue(_container);
            _container.SatisfyImportsOnce(this);

            var loader = new CommandHandlerLoader(catalog);
            _smtpServer = new SMTPServer(loader);

            foreach (var tt in _timeTables.All())
            {
                var generator = new TimeTableGenerator(tt, _mailQueue, _container);
                _generators.Add(tt.Id, generator);

                if (tt.Active) generator.Start();
            }

            _timeTables.OnAdd += OnTimeTableAdd;
            _timeTables.OnRemove += OnTimeTableRemove;

            RefreshServers();
            RefreshSenders();
        }

        private void OnTimeTableRemove(TimeTable tt)
        {
            lock (_generators)
            {
                _generators[tt.Id].Stop();
                _generators.Remove(tt.Id);
            }
        }

        private void OnTimeTableAdd(TimeTable tt)
        {
            lock (_generators)
            {
                var generator = new TimeTableGenerator(tt, _mailQueue, _container);
                _generators.Add(tt.Id, generator);

                if (tt.Active) generator.Start();
            }
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

        public void Enqueue(MailMessage mail)
        {
            var from = new MailAddress(mail.Sender);
            var to = mail.Recipients.Select(r => new MailAddress(r)).ToArray();
            var parsed = new Mail(from, to, mail.Content);
            var sendableMail = new SendableMail(parsed, _sendConnectors.DefaultConnector);

            _mailQueue.Enqueue(sendableMail, TimeSpan.Zero);
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

            _servers = _receiveConnectors.All().Select(r =>
            {
                var server = new SMTPService(r, _smtpServer, _container);
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

            var service = new ConfigurationService(_smtpServer, this, this);
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
    }
}