using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "RCPT")]
    [Export(typeof(ICommandHandler))]
    public class RCPTHandler : CommandHandlerBase
    {
        static readonly Regex ToRegex = new Regex("^TO:(" + RegularExpressions.PathPattern + "|<postmaster>)(?: (?<Params>.*))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
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
                ? Path.Postmaster
                : Path.FromMatch(match);

            transaction.GetListProperty<Path>("ForwardPath").Add(path);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}