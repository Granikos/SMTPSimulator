using System;
using System.Net;
using HydraCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HydraTest
{
    public class MailBaseTest
    {
        public SMTPResponse ExecuteCommand(string cmd, string parameters = null)
        {
            return Transaction.ExecuteCommand(new SMTPCommand(cmd, parameters));
        }

        public SMTPResponse HandleData(string data)
        {
            return Transaction.HandleData(data);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var ip = IPAddress.Loopback;
            var subnet = new IPSubnet(ip, 8);

            Server = new SMTPCore();
            Server.AddSubnet(subnet);

            SMTPResponse response;
            Transaction = Server.StartTransaction(ip, out response);
        }

        public SMTPCore Server { get; set; }

        public SMTPTransaction Transaction { get; set; }
    }
}
