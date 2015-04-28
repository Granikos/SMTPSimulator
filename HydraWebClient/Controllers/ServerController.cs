using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    public class ServerController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/<controller>
        public ServerConfig Get()
        {
            return _service.GetServerConfig();
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]ServerConfig config)
        {
            if (!_service.SetServerConfig(config))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update settings.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}