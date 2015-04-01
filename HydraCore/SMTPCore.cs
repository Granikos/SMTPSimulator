using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using HydraCore.CommandHandlers;

namespace HydraCore
{
    using CommandHandler = Func<SMTPTransaction, SMTPCommand, string, SMTPResponse>;

    public enum CommandArgMode
    {
        Forbidden, Required, Optional
    }

    public class SMTPCore
    {
        private readonly ICollection<Mailbox> _mailBoxes = new HashSet<Mailbox>();

        private readonly ICollection<IPSubnet> _allowedSubnets = new HashSet<IPSubnet>();

        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>();

        public SMTPCore()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);

            foreach (ComposablePartDefinition partDefinition in container.Catalog.Parts)
            {
                ComposablePart part = partDefinition.CreatePart();
                foreach (ExportDefinition exportDefinition in partDefinition.ExportDefinitions)
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

        public delegate void NewMessageAction(SMTPTransaction transaction, string sender, string[] recipients, string body);
        public event NewMessageAction OnNewMessage;

        internal void TriggerNewMessage(SMTPTransaction transaction, string sender, string[] recipients, string body)
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