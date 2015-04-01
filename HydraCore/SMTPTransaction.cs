using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using HydraCore.CommandHandlers;

namespace HydraCore
{
    using CommandHandler = Func<SMTPTransaction, SMTPCommand, string, SMTPResponse>;

    public class SMTPTransaction
    {
        public readonly SMTPCore Server;

        public string ClientIdentifier { get; private set; }

        public bool Initialized { get; private set; }

        public bool Closed { get; private set; }

        public bool MailInProgress { get; internal set; }

        public string ReversePath { get; internal set; }

        public List<string> ForwardPath { get; private set; }

        public bool DataMode { get; private set; }

        public SMTPTransaction(SMTPCore server)
        {
            Server = server;
            ForwardPath = new List<string>();
        }

        public delegate void CloseAction(SMTPTransaction transaction);
        public event CloseAction OnClose;

        private Func<string, SMTPResponse> _dataHandler;

        public void StartDataMode(Func<string,SMTPResponse> dataHandler)
        {
            DataMode = true;
            _dataHandler = dataHandler;
        }

        public void Reset()
        {
            DataMode = false;
            MailInProgress = false;
            ReversePath = null;
            ForwardPath.Clear();
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
            DataMode = false;

            return response;
        }
    }
}