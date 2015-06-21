using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RCPT")]
    [Export(typeof (ICommandHandler))]
    public class RCPTHandler : CommandHandlerBase
    {
        private static readonly Regex ToRegex =
            new Regex("^TO:(" + RegularExpressions.PathPattern + "|<postmaster>)(?: (?<Params>.*))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!transaction.GetProperty<bool>("MailInProgress"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var match = ToRegex.Match(parameters);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var path = match.Groups[1].Value.Equals("<postmaster>", StringComparison.InvariantCultureIgnoreCase)
                ? MailPath.Postmaster
                : MailPath.FromMatch(match);

            transaction.GetListProperty<MailPath>("ForwardPath").Add(path);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}