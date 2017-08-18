// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using bbv.Common.EventBroker;
using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;

namespace Granikos.SMTPSimulator.SmtpServer
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
            if (name == null) throw new ArgumentNullException();
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
            if (command == null) throw new ArgumentNullException();
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