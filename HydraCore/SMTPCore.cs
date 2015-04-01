using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;

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

        private readonly IDictionary<string, CommandHandler> _handlers = new Dictionary<string, CommandHandler>();
        private readonly IDictionary<string, CommandArgMode> _commandArgModes = new Dictionary<string, CommandArgMode>(); // TODO: Make this nicer

        public void AddHandler(string command, CommandHandler handler, CommandArgMode hasArgs = CommandArgMode.Optional)
        {
            _handlers.Add(command, handler);
            _commandArgModes.Add(command, hasArgs);
        }

        public CommandArgMode GetCommandArgMode(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            CommandArgMode mode;
            _commandArgModes.TryGetValue(command.Command, out mode);
            return mode;
        }

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
                transaction.Closed = true;
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

        public CommandHandler GetHandler(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            CommandHandler handler;
            _handlers.TryGetValue(command.Command, out handler);
            return handler;
        }
    }
}