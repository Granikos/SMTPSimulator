using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ReceiveConnectors")]
    public class ReceiveConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Default")]
        public ReceiveConnector Default()
        {
            return _service.GetDefaultReceiveConnector();
        }

        // GET api/ReceiveConnectors
        [HttpGet]
        [Route("")]
        public IEnumerable<ReceiveConnector> Get()
        {
            return _service.GetReceiveConnectors();
        }

        // GET api/ReceiveConnectors/Certificates
        [HttpGet]
        [Route("Certificates")]
        public IEnumerable<string> GetCertificates()
        {
            return _service.GetCertificateFiles();
        }

        // GET api/ReceiveConnectors/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var ReceiveConnector = _service.GetReceiveConnector(id);

            if (ReceiveConnector == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, ReceiveConnector);
        }

        // POST api/ReceiveConnectors
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody]ReceiveConnector ReceiveConnector)
        {
            var added = _service.AddReceiveConnector(ReceiveConnector);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/ReceiveConnectors/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]ReceiveConnector ReceiveConnector)
        {
            var updated = _service.UpdateReceiveConnector(ReceiveConnector);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/ReceiveConnectors/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteReceiveConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete Receive connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}