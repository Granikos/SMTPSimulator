using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace HydraCore
{
    using CommandHandler = Func<SMTPTransaction, SMTPCommand, string, SMTPResponse>;

    public class SMTPTransaction
    {
        public readonly SMTPCore Server;

        public string ClientIdentifier { get; protected set; }

        public bool Initialized { get; protected set; }

        public bool Closed { get; internal set; }

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

        public SMTPResponse HELO(SMTPCommand command)
        {
            Reset();

            ClientIdentifier = command.Parameters;
            Initialized = true;

            return new SMTPResponse(SMTPStatusCode.Okay, Server.Greet);
        }

        public SMTPResponse EHLO(SMTPCommand command)
        {
            Reset();

            ClientIdentifier = command.Parameters;
            Initialized = true;

            return new SMTPResponse(SMTPStatusCode.Okay, Server.Greet);
        }

        readonly Regex _fromRegex = new Regex("^FROM:<(\\w*@\\w*(?:\\.\\w*)*)?>(?: SP (.*))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        readonly Regex _toRegex = new Regex("^TO:<(\\w*@\\w*(?:\\.\\w*)*)>(?: SP (.*))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private CommandHandler _handler;

        public SMTPResponse MAIL(SMTPCommand command)
        {
            if (MailInProgress)
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence); 
            }

            var match = _fromRegex.Match(command.Parameters);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError); 
            }

            var sender = match.Groups[1].Value;
            var parameters = match.Groups[2].Value.Split(' ');

            ReversePath = sender;
            MailInProgress = true;

            return new SMTPResponse(SMTPStatusCode.Okay);
        }

        public SMTPResponse RCPT(SMTPCommand command)
        {
            if (!MailInProgress)
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var match = _toRegex.Match(command.Parameters);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var recipient = match.Groups[1].Value;
            var parameters = match.Groups[2].Value.Split(' ');

            ForwardPath.Add(recipient);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }

        public SMTPResponse DATA(SMTPCommand command, string data = null)
        {
            if (!MailInProgress || !ForwardPath.Any())
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            if (data != null)
            {
                Server.TriggerNewMessage(this, ReversePath, ForwardPath.ToArray(), data);

                Reset();

                return new SMTPResponse(SMTPStatusCode.Okay);
            }

            DataMode = true;
            return new SMTPResponse(SMTPStatusCode.StartMailInput);
        }

        public SMTPResponse RSET()
        {
            Reset();
            return new SMTPResponse(SMTPStatusCode.Okay);
        }

        public SMTPResponse NOOP()
        {
            return new SMTPResponse(SMTPStatusCode.Okay);
        }

        private void Reset()
        {
            DataMode = false;
            MailInProgress = false;
            ReversePath = null;
            ForwardPath.Clear();
        }

        public SMTPResponse VRFY(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            var searchTerm = command.Parameters;
            var boxes = Server.Mailboxes.Where(mb => mb.ToString().Contains(searchTerm)).ToArray();

            if (!boxes.Any())
            {
                return new SMTPResponse(SMTPStatusCode.MailboxUnavailiableError);
            }

            if (boxes.Length > 1)
            {
                var boxStrings = boxes.Select(mb => mb.ToString()).ToArray();
                return new SMTPResponse(SMTPStatusCode.MailboxNameNotAllowed, boxStrings);
            }

            return new SMTPResponse(SMTPStatusCode.Okay, boxes[0].ToString());
        }

        public SMTPResponse QUIT(SMTPCommand command)
        {
            if (OnClose != null) OnClose(this);

            Closed = true;

            return new SMTPResponse(SMTPStatusCode.Closing);
        }

        public SMTPResponse ExecuteCommand(SMTPCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            _handler = Server.GetHandler(command);
            var mode = Server.GetCommandArgMode(command);

            if (mode == CommandArgMode.Forbidden && !String.IsNullOrWhiteSpace(command.Parameters))
            {
                return new SMTPResponse(SMTPStatusCode.ParamError);
            }
            if (mode == CommandArgMode.Required && String.IsNullOrWhiteSpace(command.Parameters))
            {
                return new SMTPResponse(SMTPStatusCode.ParamError);
            }

            if (_handler == null)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            return _handler(this, command, null);
        }

        public SMTPResponse HandleData(SMTPCommand command, string data)
        {
            return _handler(this, command, data);
        }
    }
}