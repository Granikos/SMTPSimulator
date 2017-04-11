using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [RequiresAuth]
    [ExportMetadata("Command", "MAIL")]
    [Export(typeof (ICommandHandler))]
    public class MAILHandler : CommandHandlerBase
    {
        private static readonly Regex FromRegex =
            new Regex("^FROM:(" + RegularExpressions.PathPattern + "|<>)(?: (?<Params>.*))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (transaction.GetProperty<bool>("MailInProgress"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var match = FromRegex.Match(parameters);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var path = match.Groups[1].Value.Equals("<>") ? MailPath.Empty : MailPath.FromMatch(match);

            transaction.SetProperty("ReversePath", path);
            transaction.SetProperty("MailInProgress", true);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}