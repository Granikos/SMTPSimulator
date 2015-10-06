using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Granikos.Hydra.WebClient.HydraConfigurationService;

namespace Granikos.Hydra.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/ExternalUsers")]
    public class ExternalUsersController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/ExternalUsers
        [HttpGet]
        [Route("")]
        public ExternalUsersWithTotal All([FromUri]PagedFilter filter)
        {
            return _service.GetExternalUsers(filter.PageNumber, filter.PageSize);
        }

        // GET api/LocalUsers/Search
        [HttpGet]
        [Route("Search/{search}")]
        public IEnumerable<string> Search(string search)
        {
            return _service.SearchExternalUsers(search);
        }

        // GET api/ExternalUsers/Export
        [HttpGet]
        [Route("Export")]
        public HttpResponseMessage Export()
        {
            var stream = _service.ExportExternalUsers();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),

            };


            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "ExternalUsers.csv"
            };

            return response;
        }

        // POST api/ExternalUsers/Import
        [HttpPost]
        [Route("Import")]
        public async Task<ImportResult> Import()
        {
            var stream = await GetUploadedFileStream();

            return await _service.ImportExternalUsersAsync(stream);
        }

        // POST api/ExternalUsers/ImportWithOverwrite
        [HttpPost]
        [Route("ImportWithOverwrite")]
        public async Task<ImportResult> ImportWithOverwrite()
        {
            var stream = await GetUploadedFileStream();

            return await _service.ImportExternalUsersWithOverwriteAsync(stream);
        }

        private async Task<Stream> GetUploadedFileStream()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            var reader = await Request.Content.ReadAsMultipartAsync(provider);
            var stream = await reader.Contents.First().ReadAsStreamAsync();
            return stream;
        }

        // GET api/ExternalUsers/5
        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var user = _service.GetExternalUser(id);

            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, user);
        }

        // POST api/ExternalUsers
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Add([FromBody]ExternalUser user)
        {
            var added = _service.AddExternalUser(user);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        // PUT api/ExternalUsers/5
        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Update(int id, [FromBody]ExternalUser user)
        {
            var updated = _service.UpdateExternalUser(user);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        // DELETE api/ExternalUsers/5
        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteExternalUser(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete user.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}