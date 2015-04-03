using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using HydraCore.CommandHandlers;

namespace HydraCore
{
    public class SMTPCore
    {
        public delegate void NewMessageAction(SMTPTransaction transaction, Path sender, Path[] recipients, string body);

        private readonly ICollection<IPSubnet> _allowedSubnets = new HashSet<IPSubnet>();
        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>();
        private readonly ICollection<Mailbox> _mailBoxes = new HashSet<Mailbox>();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

        public SMTPCore()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);

            foreach (var partDefinition in container.Catalog.Parts)
            {
                var part = partDefinition.CreatePart();
                foreach (var exportDefinition in partDefinition.ExportDefinitions)
                {
                    var handler = part.GetExportedValue(exportDefinition) as ICommandHandler;

                    if (handler != null)
                    {
                        var command = exportDefinition.Metadata["Command"].ToString();

                        handler.Initialize(this);
                        _handlers.Add(command, handler);
                    }
                }
            }

            container.Dispose();
        }

        public string Banner { get; set; }
        public string Greet { get; set; }
        public string ServerName { get; set; }

        public IEnumerable<Mailbox> Mailboxes
        {
            get
            {
                foreach (var mailbox in _mailBoxes)
                {
                    yield return mailbox;
                }
            }
        }

        public T GetProperty<T>(string name)
        {
            object obj;
            _properties.TryGetValue(name, out obj);

            return obj != null ? (T) obj : default(T);
        }

        public IList<T> GetListProperty<T>(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            object obj;
            _properties.TryGetValue(name, out obj);

            if (obj != null) return (IList<T>) obj;

            var list = new List<T>();
            _properties.Add(name, list);
            return list;
        }

        public bool HasProperty(string name)
        {
            return _properties.ContainsKey(name);
        }

        public void SetProperty(string name, object value)
        {
            if (_properties.ContainsKey(name))
            {
                _properties[name] = value;
            }
            else
            {
                _properties.Add(name, value);
            }
        }

        public void AddMailBox(Mailbox mb)
        {
            _mailBoxes.Add(mb);
        }

        public void RemoveMailBox(Mailbox mb)
        {
            _mailBoxes.Remove(mb);
        }

        public bool HasMailBox(Mailbox mb)
        {
            return _mailBoxes.Contains(mb);
        }

        public void AddSubnet(IPSubnet subnet)
        {
            _allowedSubnets.Add(subnet);
        }

        public void RemoveSubnet(IPSubnet subnet)
        {
            _allowedSubnets.Remove(subnet);
        }

        public bool IsAllowedIP(IPAddress address)
        {
            return _allowedSubnets.Any(s => s.Contains(address));
        }

        public SMTPTransaction StartTransaction(IPAddress address, out SMTPResponse response)
        {
            var transaction = new SMTPTransaction(this);
            if (!IsAllowedIP(address))
            {
                response = new SMTPResponse(SMTPStatusCode.TransactionFailed);
                transaction.Close();
            }
            else
            {
                response = new SMTPResponse(SMTPStatusCode.Ready, String.Format("{0} {1}", ServerName, Banner));
            }

            return transaction;
        }

        public event NewMessageAction OnNewMessage;

        internal void TriggerNewMessage(SMTPTransaction transaction, Path sender, Path[] recipients, string body)
        {
            if (OnNewMessage != null) OnNewMessage(transaction, sender, recipients, body);
        }

        public ICommandHandler GetHandler(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            ICommandHandler handler;
            _handlers.TryGetValue(command.Command, out handler);
            return handler;
        }
    }
}