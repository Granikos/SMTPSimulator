using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [ExportMetadata("Command", "DATA")]
    [Export(typeof (ICommandHandler))]
    public class DATAHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var forwardPath = transaction.GetListProperty<MailPath>("ForwardPath");

            if (!transaction.GetProperty<bool>("MailInProgress") || forwardPath == null || !forwardPath.Any())
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            transaction.StartDataMode(DataLineHandler, data => DataHandler(transaction, data));

            return new SMTPResponse(SMTPStatusCode.StartMailInput);
        }

        public static bool DataLineHandler(string line, StringBuilder builder)
        {
            if (line.Equals(".")) return false;
            if (line.StartsWith(".")) line = line.Substring(1);

            if (builder.Length > 0) builder.Append("\r\n");

            builder.Append(line);

            return true;
        }

        public static SMTPResponse DataHandler(SMTPTransaction transaction, string data)
        {
            transaction.Server.TriggerNewMessage(transaction, transaction.GetProperty<MailPath>("ReversePath"),
                transaction.GetListProperty<MailPath>("ForwardPath").ToArray(), data);

            transaction.Reset();

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}