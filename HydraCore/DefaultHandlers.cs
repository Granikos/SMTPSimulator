using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydraCore
{
    public static class DefaultHandlers
    {
        public static void AddToServer(SMTPCore server)
        {
            server.AddHandler("HELO", (transaction, command, data) => transaction.HELO(command), CommandArgMode.Required);
            server.AddHandler("EHLO", (transaction, command, data) => transaction.EHLO(command), CommandArgMode.Required);
            server.AddHandler("MAIL", (transaction, command, data) => transaction.MAIL(command), CommandArgMode.Required);
            server.AddHandler("RCPT", (transaction, command, data) => transaction.RCPT(command), CommandArgMode.Required);
            server.AddHandler("DATA", (transaction, command, data) => transaction.DATA(command, data), CommandArgMode.Forbidden);
            server.AddHandler("RSET", (transaction, command, data) => transaction.RSET(), CommandArgMode.Forbidden);
            server.AddHandler("VRFY", (transaction, command, data) => transaction.VRFY(command), CommandArgMode.Required);
            server.AddHandler("QUIT", (transaction, command, data) => transaction.QUIT(command), CommandArgMode.Forbidden);
            server.AddHandler("NOOP", (transaction, command, data) => transaction.NOOP());
        }
    }
}
