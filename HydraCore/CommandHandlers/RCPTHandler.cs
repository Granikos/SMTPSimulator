using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RCPT")]
    [Export(typeof(ICommandHandler))]
    public class RCPTHandler : ICommandHandler
    {
        readonly Regex _toRegex = new Regex("^TO:<(\\w*@\\w*(?:\\.\\w*)*)>$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
        {
            if (!transaction.MailInProgress)
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

            transaction.ForwardPath.Add(recipient);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}