using System.Web.Http;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ImapTest")]
    public class ImapTestController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpPost]
        [Route("")]
        public ImapTestResult Send(ImapTestSettings msg)
        {
            return _service.TestImap(msg);
        }
    }
}