using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    [RoutePrefix("api/Server")]
    public class ServerController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/Server/Start
        [HttpGet]
        [Route("Start")]
        public void Start()
        {
            _service.Start();
        }

        // GET api/Server/Stop
        [HttpGet]
        [Route("Stop")]
        public void Stop()
        {
            _service.Stop();
        }

        // GET api/Server/IsRunning
        [HttpGet]
        [Route("IsRunning")]
        public bool IsRunning()
        {
            return _service.IsRunning();
        }
    }
}