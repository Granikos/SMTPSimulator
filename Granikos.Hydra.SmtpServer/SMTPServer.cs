using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using bbv.Common.EventBroker;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.SmtpServer.CommandHandlers;

namespace Granikos.NikosTwo.SmtpServer
{
    public class SMTPServer
    {
        public delegate void ConnectValidator(SMTPTransaction transaction, ConnectEventArgs connect);

        public delegate void NewMessageAction(SMTPTransaction transaction, Mail mail);

        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

        public SMTPServer(ICommandHandlerLoader loader)
        {
            EventBroker = new EventBroker();
            foreach (var handler in loader.GetModules())
            {
                _handlers.Add(handler.Item1, handler.Item2);
                handler.Item2.Initialize(this);
                EventBroker.Register(handler.Item2);
            }
        }

        public EventBroker EventBroker { get; private set; }

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
            if (value != null)
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
            else if (_properties.ContainsKey(name))
            {
                _properties.Remove(name);
            }
        }

        public event ConnectValidator OnConnect;

        public SMTPTransaction StartTransaction(IPAddress address, IReceiveSettings settings, out SMTPResponse response)
        {
            var transaction = new SMTPTransaction(this, settings);
            if (OnConnect != null)
            {
                var args = new ConnectEventArgs(address);
                OnConnect(transaction, args);

                if (args.Cancel)
                {
                    response = new SMTPResponse(args.ResponseCode ?? SMTPStatusCode.TransactionFailed);
                    transaction.Close();
                    return transaction;
                }
            }

            response = new SMTPResponse(SMTPStatusCode.Ready, settings.Banner);

            return transaction;
        }

        public event NewMessageAction OnNewMessage;

        public void TriggerNewMessage(SMTPTransaction transaction, MailPath sender, MailPath[] recipients, string body)
        {
            var mail = new Mail(sender.ToMailAdress(), recipients.Select(r => r.ToMailAdress()).Where(r => r != null),
                body);
            if (OnNewMessage != null) OnNewMessage(transaction, mail);
        }

        public ICommandHandler GetHandler(string command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            ICommandHandler handler;
            _handlers.TryGetValue(command, out handler);
            return handler;
        }

        public class ConnectEventArgs : CancelEventArgs
        {
            public ConnectEventArgs(IPAddress ip)
            {
                IP = ip;
            }

            public IPAddress IP { get; private set; }
            public SMTPStatusCode? ResponseCode { get; set; }
        }
    }
}