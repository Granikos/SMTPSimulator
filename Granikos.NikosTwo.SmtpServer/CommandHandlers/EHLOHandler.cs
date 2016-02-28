using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.NikosTwo.Core;

namespace Granikos.NikosTwo.SmtpServer.CommandHandlers
{
    [UnsecureAllowed]
    [ExportMetadata("Command", "EHLO")]
    [Export(typeof (ICommandHandler))]
    public class EHLOHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            transaction.Initialize(parameters);

            var ehlos = Server.GetListProperty<Func<SMTPTransaction, string>>("EHLOLines");
            var l = new List<string> {transaction.Settings.Greet};
            l.AddRange(ehlos.Select(e => e(transaction)).Where(e => !string.IsNullOrEmpty(e)));

            return new SMTPResponse(SMTPStatusCode.Okay, l.ToArray());
        }
    }
}