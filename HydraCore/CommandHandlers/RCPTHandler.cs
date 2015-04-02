using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RCPT")]
    [Export(typeof(ICommandHandler))]
    public class RCPTHandler : CommandHandlerBase
    {
        static readonly Regex ToRegex = new Regex("^TO:<(\\w*@\\w*(?:\\.\\w*)*)>$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (!transaction.GetProperty<bool>("MailInProgress"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            if (string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var parts = parameters.Split(' ');
            var match = ToRegex.Match(parts[0]);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var recipient = match.Groups[1].Value;

            transaction.GetListProperty<string>("ForwardPath").Add(recipient);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}