using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;

namespace Granikos.SMTPSimulator.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Certificates")]
    public class CertificateController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("By-Type/{type}")]
        public IEnumerable<string> All(string type)
        {
            return _service.GetCertificates(type);
        }

        [HttpGet]
        [Route("Types")]
        public IEnumerable<NameWithDisplayName> Types()
        {
            return _service.GetCertificateTypes();
        }

        [HttpPost]
        [Route("{name}")]
        public async Task Upload(string name)
        {
            var stream = await GetUploadedFileStream();

            if (!_service.UploadCertificate(name, stream))
            {
                throw new Exception("Could not upload certificate!");
            }
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
        [Route("Download/{name}")]
        public HttpResponseMessage Download(string name)
        {
            var stream = _service.DownloadCertificate(name);

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

        [HttpDelete]
        [Route("{name}")]
        public HttpResponseMessage Delete(string name)
        {
            if (!_service.DeleteCertificate(name))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Could not delete certificate.");
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}