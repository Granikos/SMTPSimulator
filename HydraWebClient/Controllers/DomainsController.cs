using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    [RoutePrefix("api/Domains")]
    public class DomainsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/Domains
        [HttpGet]
        [Route("")]
        public IEnumerable<Domain> Get()
        {
            return _service.GetDomains();
        }

        // GET api/Domains/5
        [HttpGet]
        [Route("{domainName}")]
        public HttpResponseMessage Get(string domainName)
        {
            var domain = _service.GetDomain(domainName);

            if (domain == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find domain.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, domain);
        }

        // POST api/Domains
        [HttpPost]
        [Route("{domainName}")]
        public HttpResponseMessage Post(string domainName)
        {
            var added = _service.AddDomain(domainName);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add domain.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/Domains/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]Domain domain)
        {
            var updated = _service.UpdateDomain(domain);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update domain.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/Domains/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteDomain(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete domain.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}