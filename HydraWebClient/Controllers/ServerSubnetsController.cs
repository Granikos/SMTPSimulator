using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    public class ServerSubnetsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/<controller>
        public IEnumerable<ServerSubnetConfiguration> Get()
        {
            return _service.GetSubnets();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(int id)
        {
            var subnet = _service.GetSubnet(id);

            if (subnet == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find subnet.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, subnet);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]ServerSubnetConfiguration subnet)
        {
            var added = _service.AddSubnet(subnet);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add subnet.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id, [FromBody]ServerSubnetConfiguration subnet)
        {
            var updated = _service.UpdateSubnet(subnet);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update subnet.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteSubnet(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete subnet.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}