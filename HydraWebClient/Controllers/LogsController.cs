using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using HydraWebClient.HydraConfigurationService;

namespace HydraWebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Logs")]
    public class LogsController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/Logs/List
        [HttpGet]
        [Route("List")]
        public string[] List()
        {
            return _service.GetLogNames();
        }

        // GET api/Logs/Get/{name}
        [HttpGet]
        [Route("Get/{*name}")]
        public HttpResponseMessage Get(string name)
        {
            var stream = _service.GetLogFile(name);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(name.EndsWith(".csv") ? "text/csv" : "text/plain");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = name
            };

            return response;
        }
    }
}