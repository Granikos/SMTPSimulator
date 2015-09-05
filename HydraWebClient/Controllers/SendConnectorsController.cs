using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/SendConnectors")]
    public class SendConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Empty")]
        public SendConnector Empty()
        {
            return _service.GetEmptySendConnector();
        }

        [HttpGet]
        [Route("Default")]
        public SendConnector Default()
        {
            return _service.GetDefaultSendConnector();
        }

        [HttpPost]
        [Route("Default/{id:int}")]
        public HttpResponseMessage SetDefault(int id)
        {
            if (!_service.SetDefaultSendConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update default send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET api/SendConnectors
        [HttpGet]
        [Route("")]
        public IEnumerable<SendConnector> Get()
        {
            return _service.GetSendConnectors();
        }

        // GET api/SendConnectors/Certificates
        [HttpGet]
        [Route("Certificates")]
        public IEnumerable<string> GetCertificates()
        {
            return _service.GetCertificateFiles();
        }

        // GET api/SendConnectors/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var sendConnector = _service.GetSendConnector(id);

            if (sendConnector == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, sendConnector);
        }

        // POST api/SendConnectors
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody]SendConnector sendConnector)
        {
            var added = _service.AddSendConnector(sendConnector);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/SendConnectors/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]SendConnector sendConnector)
        {
            var updated = _service.UpdateSendConnector(sendConnector);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/SendConnectors/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteSendConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete send connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}