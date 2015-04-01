using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "MAIL")]
    [Export(typeof(ICommandHandler))]
    public class MAILHandler : ICommandHandler
    {
        readonly Regex _fromRegex = new Regex("^FROM:<(\\w*@\\w*(?:\\.\\w*)*)?>$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
        {
            if (transaction.MailInProgress)
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var parts = parameters.Split(' ');

            if (!parts.Any())
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var match = _fromRegex.Match(parts[0]);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var sender = match.Groups[1].Value;

            transaction.ReversePath = sender;
            transaction.MailInProgress = true;

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}