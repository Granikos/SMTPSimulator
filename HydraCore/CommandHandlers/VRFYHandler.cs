using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "VRFY")]
    [Export(typeof(ICommandHandler))]
    public class VRFYHandler : ICommandHandler
    {
        public SMTPResponse Execute(SMTPTransaction transaction, string parameters, string data)
        {
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var boxes = transaction.Server.Mailboxes.Where(mb => mb.ToString().Contains(parameters)).ToArray();

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