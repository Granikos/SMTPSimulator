using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceProcess;
using Granikos.Hydra.Core;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;
using Granikos.Hydra.Service.PriorityQueue;
using Granikos.Hydra.Service.Retention;
using Granikos.Hydra.Service.TimeTables;
using Granikos.Hydra.SmtpClient;
using Granikos.Hydra.SmtpServer;
using Granikos.Hydra.SmtpServer.CommandHandlers;
using log4net;
using log4net.Appender;
using MailMessage = Granikos.Hydra.Service.ConfigurationService.Models.MailMessage;

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
        private readonly RetentionManager _retentionManager;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(HydraService));

        public HydraService()
        {
            Logger.Info("The Nikos 2 Service is starting...");

            FixLoggerPaths();
            InitializeDataDirectory();
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

            _timeTables.OnTimeTableAdd += OnTimeTableAdd;
            _timeTables.OnTimeTableRemove += OnTimeTableRemove;

            _retentionManager = new RetentionManager();

            RefreshServers();
            RefreshSenders();

            Logger.Info("The Nikos 2 Service has started.");
        }

        private static void InitializeDataDirectory()
        {
            // TODO: Customizable folder name
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dataDir = Path.Combine(appData, "Hydra");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
        }

        private static void FixLoggerPaths()
        {
            var exeLocation = Assembly.GetEntryAssembly().Location;
            var basePath = Path.GetFullPath(Path.GetDirectoryName(exeLocation));

            var appenders = LogManager.GetAllRepositories()
                .SelectMany(r => r.GetAppenders())
                .OfType<FileAppender>();
            foreach (var fileAppender in appenders)
            {
                fileAppender.File = Path.Combine(basePath, fileAppender.File);
                fileAppender.ActivateOptions();
            }
        }

        private void OnTimeTableRemove(ITimeTable tt)
        {
            lock (_generators)
            {
                _generators[tt.Id].Stop();
                _generators.Remove(tt.Id);
            }
        }

        private void OnTimeTableAdd(ITimeTable tt)
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
            Logger.Info("Stopping SMTP servers...");
            if (Running && _servers != null)
            {
                foreach (var server in _servers)
                {
                    server.Stop();
                }
            }

            Running = false;
            Logger.Info("SMTP servers stopped.");
        }

        public void StartSMTPServers()
        {
            Logger.Info("Starting SMTP servers...");
            if (!Running && _servers != null)
            {
                foreach (var server in _servers)
                {
                    server.Start();
                }
            }

            Running = true;
            Logger.Info("SMTP servers started.");
        }

        public void StopMessageProcessing()
        {
            if (_senders != null)
            {
                Logger.Info("Stopping mail sending...");
                foreach (var sender in _senders)
                {
                    sender.Stop();
                }
                Logger.Info("Stopped mail sending.");
            }
        }

        public void StartMessageProcessing()
        {
            if (_senders != null)
            {
                Logger.Info("Starting mail sending...");
                foreach (var sender in _senders)
                {
                    sender.Start();
                }
                Logger.Info("Started mail sending.");
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
            Logger.Info("Service starting...");
            StartSMTPServers();
            StartMessageProcessing();

            if (_host != null)
            {
                _host.Close();
            }

            var service = new ConfigurationServiceImpl(_smtpServer, this, this);
            _container.ComposeParts(service);

            _host = new WebServiceHost(service);

            _host.Open();

            _retentionManager.Start();
            Logger.Info("Service started.");
        }

        protected override void OnStop()
        {
            Logger.Info("Service stopping...");
            StopSMTPServers();
            StopMessageProcessing();

            if (_host != null)
            {
                _host.Close();
                _host = null;
            }

            _retentionManager.Stop();
            Logger.Info("Service stopped...");
        }
    }
}