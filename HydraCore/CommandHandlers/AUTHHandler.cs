using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using bbv.Common.EventBroker;
using bbv.Common.EventBroker.Handlers;
using HydraCore.AuthMethods;

namespace HydraCore.CommandHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresAuthAttribute : Attribute
    {
        
    }

    [CommandHandler(Command = "AUTH")]
    public class AUTHHandler : CommandHandlerBase
    {
        [Import]
        public IAuthMethodLoader Loader; // TODO: Find a way to initialize through constructor with MEF

        [EventSubscription("CommandExecution", typeof(Publisher))]
        public void OnCommandExecute(object sender, CommandExecuteEventArgs args)
        {
            var type = args.Handler.GetType();
            var requiresAuth = type.GetCustomAttributes(typeof(RequiresAuthAttribute), false).Any();

            if (requiresAuth && !args.Transaction.GetProperty<bool>("Authenticated"))
            {
                args.Response = new SMTPResponse(SMTPStatusCode.AuthRequired);
            }
        }

        private readonly Dictionary<string, IAuthMethod> _authMethods = new Dictionary<string, IAuthMethod>();

        public override void Initialize(SMTPCore core)
        {
            base.Initialize(core);

            foreach (var method in Loader.GetModules())
            {
                _authMethods.Add(method.Item1.ToUpperInvariant(), method.Item2);
            }

            if (_authMethods.Any())
            {
                var methods = String.Join(" ", _authMethods.Keys);

                core.GetListProperty<string>("EHLOLines").Add("AUTH " + methods);
            }
        }

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (String.IsNullOrEmpty(parameters))
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
                if (parts[1].Equals("=")) initialResponse = String.Empty;
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

        protected string Base64Encode(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }
        protected string Base64Decode(string str)
        {
            Contract.Requires<ArgumentNullException>(str != null);
            return Encoding.ASCII.GetString(Convert.FromBase64String(str));
        }

        private bool DataLineHandler(string line, StringBuilder builder)
        {
            builder.Append(line);
            return false;
        }

        private SMTPResponse DataHandler(SMTPTransaction transaction, string reponse, IAuthMethod method)
        {
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

        private SMTPResponse HandleResponse(SMTPTransaction transaction, string decodedReponse, IAuthMethod method)
        {
            string challenge;
            if (!method.ProcessResponse(transaction, decodedReponse, out challenge))
            {
                return new SMTPResponse(SMTPStatusCode.AuthFailed, challenge != null ? new[] { challenge } : new string[0]);
            }

            if (challenge != null)
            {
                transaction.StartDataMode(DataLineHandler, s => DataHandler(transaction, s, method));

                return new SMTPResponse(SMTPStatusCode.StartMailInput, Base64Encode(challenge));
            }

            transaction.SetProperty("Authenticated", true, true);

            return new SMTPResponse(SMTPStatusCode.AuthSuccess);
        }
    }
}