using System.ComponentModel.Composition;
using System.Linq;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [ExportMetadata("Command", "VRFY")]
    [Export(typeof (ICommandHandler))]
    public class VRFYHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var mailboxes = Server.GetListProperty<Mailbox>("Mailboxes");
            var boxes = mailboxes.Where(mb => mb.ToString().Contains(parameters)).ToArray();

            if (!boxes.Any())
            {
                return new SMTPResponse(SMTPStatusCode.MailboxUnavailiableError);
            }

            if (boxes.Length > 1)
            {
                var boxStrings = boxes.Select(mb => mb.ToString()).ToArray();
                return new SMTPResponse(SMTPStatusCode.MailboxNameNotAllowed, boxStrings);
            }

            return new SMTPResponse(SMTPStatusCode.Okay, boxes[0].ToString());
        }
    }
}