using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Text;

namespace HydraCore
{
    public class SMTPTransaction
    {
        private readonly IReceiveSettings _settings;

        public IReceiveSettings Settings
        {
            get { return _settings; }
        }

        public delegate void CloseAction(SMTPTransaction transaction);

        private readonly IDictionary<string, object> _permanentProperties = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private Func<string, SMTPResponse> _dataHandler;
        private Func<string, StringBuilder, bool> _dataLineHandler;
         
        public SMTPTransaction(SMTPCore server, IReceiveSettings settings)
        {
            Contract.Requires<ArgumentNullException>(server != null);
            Contract.Requires<ArgumentNullException>(settings != null);

            _settings = settings;
            Server = server;
        }

        public SMTPCore Server { get; private set; }
        public string ClientIdentifier { get; private set; }
        public bool Initialized { get; private set; }
        public bool Closed { get; private set; }
        public bool TLSEnabled { get; set; }

        public bool InDataMode
        {
            get { return _dataHandler != null; }
        }

        public T GetProperty<T>(string name)
        {
            object obj;
            if (!_properties.TryGetValue(name, out obj))
            {
                _permanentProperties.TryGetValue(name, out obj);
            }

            return obj != null ? (T) obj : default(T);
        }

        public IList<T> GetListProperty<T>(string name, bool permanent = false)
        {
            Contract.Requires<ArgumentNullException>(name != null);

            var property = GetProperty<IList<T>>(name);

            if (property != null)
            {
                return property;
            }

            var target = permanent ? _permanentProperties : _properties;

            var list = new List<T>();
            target.Add(name, list);
            return list;
        }

        public bool HasProperty(string name)
        {
            return _properties.ContainsKey(name) || _permanentProperties.ContainsKey(name);
        }

        public void SetProperty(string name, object value, bool permanent = false)
        {
            var target = permanent ? _permanentProperties : _properties;

            if (value != null)
            {
                if (target.ContainsKey(name))
                {
                    target[name] = value;
                }
                else
                {
                    target.Add(name, value);
                }
            }
            else if (target.ContainsKey(name))
            {
                target.Remove(name);
            }
        }

        public event EventHandler<CancelEventArgs> OnStartTLS;

        public event CloseAction OnClose;

        public void StartDataMode(Func<string, StringBuilder, bool> dataLineHandler,
            Func<string, SMTPResponse> dataHandler)
        {
            Contract.Requires<ArgumentNullException>(dataLineHandler != null);
            Contract.Requires<ArgumentNullException>(dataHandler != null);

            _dataLineHandler = dataLineHandler;
            _dataHandler = dataHandler;
        }

        public void Reset()
        {
            _properties.Clear();
            _dataHandler = null;
            _dataLineHandler = null;
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

            var handler = Server.GetHandler(command.Command);

            if (handler == null)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            return handler.Execute(this, command.Parameters);
        }

        public bool HandleDataLine(string line, StringBuilder builder)
        {
            if (_dataLineHandler == null) throw new InvalidOperationException("Not in data mode!");
            return _dataLineHandler(line, builder);
        }

        public SMTPResponse HandleData(string data)
        {
            if (_dataHandler == null) throw new InvalidOperationException("Not in data mode!");
            var handler = _dataHandler;
            _dataHandler = null;
            _dataLineHandler = null;

            return handler(data);
        }

        public void StartTLS(CancelEventArgs e)
        {
            var handler = OnStartTLS;
            if (handler != null) handler(this, e);
        }
    }
}