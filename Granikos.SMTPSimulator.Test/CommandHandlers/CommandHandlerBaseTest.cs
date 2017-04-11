using Granikos.SMTPSimulator.Core;
using Granikos.SMTPSimulator.SmtpServer;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers;
using Granikos.SMTPSimulator.SmtpServer.CommandHandlers.Fakes;
using Xunit;

namespace SMTPSimulatorTest.CommandHandlers
{
    public class CommandHandlerBaseTest : TestBase
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestOnExecuteTrigger(bool overwriteReponse)
        {
            const string expectedParams = "fu bar";
            SMTPTransaction actualTransaction = null;
            string actualParams = null;
            var responseA = new SMTPResponse(SMTPStatusCode.Okay);
            var responseB = new SMTPResponse(SMTPStatusCode.TransactionFailed);
            var expectedResponse = overwriteReponse ? responseB : responseA;

            var handler = new StubCommandHandlerBase
            {
                DoExecuteSMTPTransactionString = (transaction, s) =>
                {
                    actualTransaction = transaction;
                    actualParams = s;

                    return responseA;
                }
            };

            object actualSender = null;
            string eventParams = null;
            SMTPTransaction eventTransaction = null;
            ICommandHandler eventHandler = null;
            SMTPResponse eventResponse = null;

            handler.OnExecute += (sender, args) =>
            {
                actualSender = sender;
                eventParams = args.Parameters;
                eventTransaction = args.Transaction;
                eventHandler = args.Handler;
                eventResponse = args.Response;

                if (overwriteReponse) args.Response = responseB;
            };

            var response = handler.Execute(Transaction, expectedParams);

            if (!overwriteReponse)
            {
                Assert.Equal(expectedParams, actualParams);
                Assert.Equal(Transaction, actualTransaction);
            }


            Assert.Same(expectedResponse, response);

            Assert.Same(handler, actualSender);
            Assert.Equal(Transaction, eventTransaction);
            Assert.Equal(expectedParams, eventParams);
            Assert.Equal(handler, eventHandler);
            Assert.Null(eventResponse);
        }
    }
}