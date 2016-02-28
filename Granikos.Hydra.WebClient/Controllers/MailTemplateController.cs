using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/MailTemplates")]
    public class MailTemplateController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("")]
        public IEnumerable<MailTemplate> All()
        {
            return _service.GetMailTemplates();
        }

        [HttpGet]
        [Route("Empty")]
        public MailTemplate Empty()
        {
            return new MailTemplate();
        }

        [HttpGet]
        [Route("Search/{search}")]
        public IEnumerable<string> Search(string search)
        {
            return _service.SearchLocalUsers(search);
        }

        [HttpGet]
        [Route("Export/{id}")]
        public HttpResponseMessage Export(int id, string name)
        {
            var stream = _service.ExportMailTemplate(id);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),

            };


            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = (name ?? "MailTemplate") + ".xml"
            };

            return response;
        }

        [HttpPost]
        [Route("Import")]
        public async Task<MailTemplate> Import()
        {
            var stream = await GetUploadedFileStream();

            return _service.ImportMailTemplate(stream);
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

        [HttpGet]
        [Route("{id:int}")]
        public HttpResponseMessage Get(int id)
        {
            var template = _service.GetMailTemplate(id);

            if (template == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not find mail template.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, template);
        }

        [HttpPost]
        [Route("")]
        public HttpResponseMessage Add([FromBody]MailTemplate template)
        {
            var added = _service.AddMailTemplate(template);

            if (added == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not add mail template.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, added);
        }

        [HttpPut]
        [Route("{id:int}")]
        public HttpResponseMessage Update(int id, [FromBody]MailTemplate template)
        {
            var updated = _service.UpdateMailTemplate(template);

            if (updated == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not update mail template.");
            }

            return Request.CreateResponse(HttpStatusCode.OK, updated);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public HttpResponseMessage Delete(int id)
        {
            if (!_service.DeleteMailTemplate(id))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete mail template.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}