using System.Web.Http;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ConnectionTest")]
    public class ConnectionTestController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpPost]
        [Route("Imap")]
        public ImapTestResult TestImap(ConnectionTestSettings settings)
        {
            return _service.TestImap(settings);
        }

        [HttpPost]
        [Route("Pop3")]
        public Pop3TestResult TestPop3(ConnectionTestSettings settings)
        {
            return _service.TestPop3(settings);
        }
    }
}