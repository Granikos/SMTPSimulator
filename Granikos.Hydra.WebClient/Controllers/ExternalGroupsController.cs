using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Granikos.Hydra.WebClient.HydraConfigurationService;

namespace Granikos.Hydra.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ExternalGroups")]
    public class ExternalGroupsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("")]
        public IEnumerable<MailboxGroup> Get()
        {
            return _service.GetExternalGroups();
        }

        [HttpGet]
        [Route("WithCounts")]
        public object GetWithCounts()
        {
            var total = _service.GetExternalUserCount();

            return new
            {
                Items = _service.GetExternalGroups()
                    .Select(g => new
                    {
                        Name = g.Name,
                        Count = g.MailboxIds.Length
                    }).ToArray(),
                MailboxTotal = total
            };
        }

        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(int id)
        {
            var domain = _service.GetExternalGroup(id);

            if (domain == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, domain);
        }

        [HttpPost]
        [Route("{*name}")]
        public HttpResponseMessage Post(string name)
        {
            var added = _service.AddExternalGroup(name);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Put(int id, [FromBody]MailboxGroup @group)
        {
            var updated = _service.UpdateExternalGroup(@group);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteExternalGroup(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete external user group.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}