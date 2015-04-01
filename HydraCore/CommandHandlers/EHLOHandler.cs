using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace HydraCore.CommandHandlers
{
    [ExportMetadata("Command", "EHLO")]
    [Export(typeof(ICommandHandler))]
    public class EHLOHandler : CommandHandlerBase
    {
        private string[] _lines;

        private string[] Lines
        {
            get
            {
                if (_lines == null)
                {
                    var l = new List<string> { Server.Greet};
                    l.AddRange(Server.GetListProperty<string>("EHLOLines"));
                    _lines = l.ToArray();
                }

                return _lines;
            }
        }

        public override SMTPResponse Execute(SMTPTransaction transaction, string parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            transaction.Reset();
            transaction.Initialize(parameters);

            return new SMTPResponse(SMTPStatusCode.Okay, Lines);
        }
    }
}