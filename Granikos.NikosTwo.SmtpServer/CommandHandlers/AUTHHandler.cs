using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using bbv.Common.EventBroker;
using bbv.Common.EventBroker.Handlers;
using Granikos.NikosTwo.Core;
using Granikos.NikosTwo.SmtpServer.AuthMethods;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresAuthAttribute : Attribute
    {
    }

    [CommandHandler(Command = "AUTH")]
    public class AUTHHandler : CommandHandlerBase
    {
        private readonly Dictionary<string, IAuthMethod> _authMethods = new Dictionary<string, IAuthMethod>();

        [ImportingConstructor]
        public AUTHHandler([Import] IAuthMethodLoader loader)
        {
            Contract.Requires<ArgumentNullException>(loader != null);

            foreach (var method in loader.GetModules())
            {
                _authMethods.Add(method.Item1.ToUpperInvariant(), method.Item2);
            }
        }

        [EventSubscription("CommandExecution", typeof (Publisher))]
        public void OnCommandExecute(object sender, CommandExecuteEventArgs args)
        {
            var type = args.Handler.GetType();
            var actionRequiresAuth = type.GetCustomAttributes(typeof (RequiresAuthAttribute), false).Any();
            var transactionRequiresAuth = args.Transaction.Settings.RequireAuth;

            if (actionRequiresAuth && transactionRequiresAuth && !args.Transaction.GetProperty<bool>("Authenticated"))
            {
                args.Response = new SMTPResponse(SMTPStatusCode.AuthRequired);
            }
        }

        public override void Initialize(SMTPServer server)
        {
            base.Initialize(server);

            if (_authMethods.Any())
            {
                var methods = string.Join(" ", _authMethods.Keys);

                server.GetListProperty<Func<SMTPTransaction, string>>("EHLOLines").Add(transaction => "AUTH " + methods);
            }
        }

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var parts = parameters.Split(' ');

            if (parts.Length > 2) return new SMTPResponse(SMTPStatusCode.SyntaxError);

            if (transaction.GetProperty<bool>("Authenticated"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            IAuthMethod method;

            if (!_authMethods.TryGetValue(parts[0].ToUpperInvariant(), out method))
            {
                return new SMTPResponse(SMTPStatusCode.ParamNotImplemented);
            }

            string initialResponse = null;

            if (parts.Length > 1)
            {
                if (parts[1].Equals("=")) initialResponse = string.Empty;
                else
                {
                    try
                    {
                        initialResponse = Base64Decode(parts[1]);
                    }
                    catch (FormatException)
                    {
                        return new SMTPResponse(SMTPStatusCode.SyntaxError);
                    }
                }
            }

            return HandleResponse(transaction, initialResponse, method);
        }

        protected static string Base64Encode(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        protected static string Base64Decode(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            return Encoding.ASCII.GetString(Convert.FromBase64String(str));
        }

        public static bool DataLineHandler(string line, StringBuilder builder)
        {
            builder.Append(line);
            return false;
        }

        public static SMTPResponse DataHandler(SMTPTransaction transaction, string reponse, IAuthMethod method)
        {
            Contract.Requires<ArgumentNullException>(transaction != null);
            Contract.Requires<ArgumentNullException>(reponse != null);
            Contract.Requires<ArgumentNullException>(method != null);

            if (reponse.Equals("*"))
            {
                method.Abort(transaction);
                return new SMTPResponse(SMTPStatusCode.ParamError);
            }

            string decoded;
            try
            {
                decoded = Base64Decode(reponse);
            }
            catch (FormatException)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            return HandleResponse(transaction, decoded, method);
        }

        public static SMTPResponse HandleResponse(SMTPTransaction transaction, string decodedReponse, IAuthMethod method)
        {
            string challenge;
            if (!method.ProcessResponse(transaction, decodedReponse, out challenge))
            {
                return new SMTPResponse(SMTPStatusCode.AuthFailed, challenge != null ? new[] {challenge} : new string[0]);
            }

            if (challenge != null)
            {
                transaction.StartDataMode(DataLineHandler, s => DataHandler(transaction, s, method));

                return new SMTPResponse(SMTPStatusCode.AuthContinue, Base64Encode(challenge));
            }

            transaction.SetProperty("Authenticated", true, true);

            return new SMTPResponse(SMTPStatusCode.AuthSuccess);
        }
    }
}