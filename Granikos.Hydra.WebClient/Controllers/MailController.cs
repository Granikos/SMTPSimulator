using System.Web.Http;
using Granikos.Hydra.Service.ConfigurationService.Models;

namespace Granikos.Hydra.WebClient.Controllers
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