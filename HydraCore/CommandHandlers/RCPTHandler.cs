using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RCPT")]
    [Export(typeof(ICommandHandler))]
    public class RCPTHandler : CommandHandlerBase
    {
        readonly Regex _toRegex = new Regex("^TO:<(\\w*@\\w*(?:\\.\\w*)*)>$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (!transaction.GetProperty<bool>("MailInProgress"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var parts = parameters.Split(' ');

            if (!parts.Any())
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var match = _toRegex.Match(parts[0]);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var recipient = match.Groups[1].Value;

            if (!transaction.HasProperty("ForwardPath"))
            {
                transaction.SetProperty("ForwardPath", new List<string>());
            }

            transaction.GetProperty<List<string>>("ForwardPath").Add(recipient);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}