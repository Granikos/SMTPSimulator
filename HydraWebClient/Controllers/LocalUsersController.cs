using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    [Authorize]
    public class LocalUsersController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/<controller>
        public IEnumerable<LocalUser> Get()
        {
            return _service.GetLocalUsers();
        }

        // GET api/<controller>/5
        public HttpResponseMessage Get(int id)
        {
            var user = _service.GetLocalUser(id);

            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]LocalUser user)
        {
            var added = _service.AddLocalUser(user);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/<controller>/5
        public HttpResponseMessage Put(int id, [FromBody]LocalUser user)
        {
            var updated = _service.UpdateLocalUser(user);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteLocalUser(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}