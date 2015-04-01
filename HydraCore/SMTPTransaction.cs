using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace HydraCore
{
    using CommandHandler = Func<SMTPTransaction, SMTPCommand, string, SMTPResponse>;

    public class SMTPTransaction
    {
        public readonly SMTPCore Server;

        public string ClientIdentifier { get; private set; }

        public bool Initialized { get; private set; }

        public bool Closed { get; private set; }

        public bool InDataMode { get { return _dataHandler != null; } }

        public SMTPTransaction(SMTPCore server)
        {
            Server = server;
        }

        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _permanentProperties = new Dictionary<string, object>();

        public T GetProperty<T>(string name)
        {
            object obj;
            if (!_properties.TryGetValue(name, out obj))
            {
                _permanentProperties.TryGetValue(name, out obj);
            }

            return obj != null? (T) obj : default(T);
        }

        public bool HasProperty(string name)
        {
            return _properties.ContainsKey(name) || _permanentProperties.ContainsKey(name);
        }

        public void SetProperty(string name, object value, bool permanent = false)
        {
            var target = permanent ? _permanentProperties : _properties;


                if (target.ContainsKey(name))
                {
                    target[name] = value;
                }
                else
                {
                    target.Add(name, value);
              }
        }

        public delegate void CloseAction(SMTPTransaction transaction);
        public event CloseAction OnClose;

        private Func<string, SMTPResponse> _dataHandler;

        public void StartDataMode(Func<string,SMTPResponse> dataHandler)
        {
            _dataHandler = dataHandler;
        }

        public void Reset()
        {
            _properties.Clear();
            _dataHandler = null;
        }

        public void Close()
        {
            if (OnClose != null) OnClose(this);

            Closed = true;
        }

        public void Initialize(string client)
        {
            ClientIdentifier = client;
            Initialized = true;
        }

        public SMTPResponse ExecuteCommand(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            var handler = Server.GetHandler(command);

            if (handler == null)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            return handler.Execute(this, command.Parameters);
        }

        public SMTPResponse HandleData(string data)
        {
            var response = _dataHandler(data);

            _dataHandler = null;

            return response;
        }
    }
}