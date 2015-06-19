using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    [RoutePrefix("api/RecieveConnectors")]
    public class RecieveConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Default")]
        public RecieveConnector Default()
        {
            return _service.GetDefaultRecieveConnector();
        }

        // GET api/RecieveConnectors
        [HttpGet]
        [Route("")]
        public IEnumerable<RecieveConnector> Get()
        {
            return _service.GetRecieveConnectors();
        }

        // GET api/RecieveConnectors/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var recieveConnector = _service.GetRecieveConnector(id);

            if (recieveConnector == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find recieve connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, recieveConnector);
        }

        // POST api/RecieveConnectors
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post([FromBody]RecieveConnector recieveConnector)
        {
            var added = _service.AddRecieveConnector(recieveConnector);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add recieve connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/RecieveConnectors/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]RecieveConnector recieveConnector)
        {
            var updated = _service.UpdateRecieveConnector(recieveConnector);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update recieve connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/RecieveConnectors/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteRecieveConnector(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete recieve connector.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}