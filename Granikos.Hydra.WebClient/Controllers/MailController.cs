using System.Web.Http;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Mail")]
    public class MailController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // PUT api/Mail/Send
        [HttpPost]
        [Route("Send")]
        public void Send(MailMessage msg)
        {
            _service.SendMail(msg);
        }
    }
}