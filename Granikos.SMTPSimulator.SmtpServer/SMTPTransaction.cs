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
using System.Text;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer
{
    public class SMTPTransaction
    {
        public delegate void CloseAction(SMTPTransaction transaction);

        private readonly IDictionary<string, object> _permanentProperties = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly IReceiveSettings _settings;
        private Func<string, SMTPResponse> _dataHandler;
        private Func<string, StringBuilder, bool> _dataLineHandler;

        public SMTPTransaction(SMTPServer server, IReceiveSettings settings)
        {
            if (server == null) throw new ArgumentNullException();
            if (settings == null) throw new ArgumentNullException();

            _settings = settings;
            Server = server;
        }

        public IReceiveSettings Settings
        {
            get { return _settings; }
        }

        public SMTPServer Server { get; private set; }
        public string ClientIdentifier { get; private set; }
        public bool Initialized { get; private set; }
        public bool Closed { get; private set; }
        public bool TLSActive { get; set; }

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
            if (name == null) throw new ArgumentNullException();

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
            if (dataLineHandler == null) throw new ArgumentNullException();
            if (dataHandler == null) throw new ArgumentNullException();

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
            if (command == null) throw new ArgumentNullException();

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