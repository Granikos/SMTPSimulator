using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Domains")]
    public class DomainsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("")]
        public IEnumerable<Domain> Get()
        {
            return _service.GetDomains();
        }

        [HttpGet]
        [Route("WithMailboxCount")]
        public IEnumerable<DomainWithMailboxCount> GetWithMailboxCount()
        {
            return _service.GetDomainsWithMailboxCount();
        }

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

        static readonly Regex DomainRegex = new Regex(@"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [HttpPost]
        [Route("{*domainName}")]
        public HttpResponseMessage Post(string domainName)
        {
            if (!DomainRegex.IsMatch(domainName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid domain name.");
            }

            var added = _service.AddDomain(domainName);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add domain.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

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