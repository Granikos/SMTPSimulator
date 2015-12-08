using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Granikos.Hydra.Service.ConfigurationService.Models;

namespace Granikos.Hydra.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Attachments")]
    public class AttachmentController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("")]
        public IEnumerable<Attachment> All()
        {
            return _service.GetAttachments();
        }

        [HttpPost]
        [Route("{name}")]
        public async Task Upload(string name, int size)
        {
            var stream = await GetUploadedFileStream();

            _service.UploadAttachment(name, size, stream);
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
        [Route("{name}")]
        public HttpResponseMessage Download(string name)
        {
            var stream = _service.DownloadAttachment(name);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream),

            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(name));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = name
            };

            return response;
        }

        [HttpPut]
        [Route("{name}")]
        public HttpResponseMessage Put(string name, string newName)
        {
            if (!_service.RenameAttachment(name, newName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete rename attachment.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
            ;
        }

        [HttpDelete]
        [Route("{name}")]
        public HttpResponseMessage Delete(string name)
        {
            if (!_service.DeleteAttachment(name))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete attachment.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
