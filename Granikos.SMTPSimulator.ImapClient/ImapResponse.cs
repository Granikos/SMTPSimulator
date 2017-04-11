using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Granikos.SMTPSimulator.ImapClient
{
    public class ImapResponse
    {
        public ImapResponse(string response)
        {
            Response = response;
        }

        public String Response { get; private set; }
    }
}
