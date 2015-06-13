using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    public class RecieveConnectorsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // TODO: Renaming

        // GET api/<controller>
        public IEnumerable<RecieveConnector> Get()
        {
            return _service.GetServerBindings();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(int id)
        {
            var serverBinding = _service.GetServerBinding(id);

            if (serverBinding == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find server binding.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, serverBinding);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]RecieveConnector serverBinding)
        {
            var added = _service.AddServerBinding(serverBinding);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add server binding.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id, [FromBody]RecieveConnector serverBinding)
        {
            var updated = _service.UpdateServerBinding(serverBinding);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update server binding.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteServerBinding(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete server binding.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}