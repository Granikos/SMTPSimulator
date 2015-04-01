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

        public SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (transaction.GetProperty<bool>("MailInProgress"))
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

            transaction.SetProperty("ReversePath", sender);
            transaction.SetProperty("MailInProgress", true);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}