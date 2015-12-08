using System;
using System.Reflection;
using System.Web.Http;
using Granikos.Hydra.Service.ConfigurationService.Models;

namespace Granikos.Hydra.WebClient.Controllers
{
    public struct ExtendedVersionInfo
    {
        public DateTime ServiceBuildDate { get; set; }

        public System.Version ServiceVersion { get; set; }

        public DateTime UiBuildDate { get; set; }

        public System.Version UiVersion { get; set; }
    }

    // [Authorize]
    [RoutePrefix("api/Server")]
    public class ServerController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        // GET api/Server/Start
        [HttpGet]
        [Route("Start")]
        public void Start()
        {
            _service.Start();
        }

        // GET api/Server/Stop
        [HttpGet]
        [Route("Stop")]
        public void Stop()
        {
            _service.Stop();
        }

        // GET api/Server/IsRunning
        [HttpGet]
        [Route("IsRunning")]
        public bool IsRunning()
        {
            return _service.IsRunning();
        }

        // GET api/Server/Version
        [HttpGet]
        [Route("Version")]
        public ExtendedVersionInfo GetVersion()
        {
            var version = _service.GetVersionInfo();
            var assembly = typeof(ServerController).Assembly;
            var uiVersion = assembly.GetName().Version;
            var date = assembly.GetBuildDate();

            return new ExtendedVersionInfo
            {
                ServiceBuildDate = version.BuildDate,
                ServiceVersion = version.Version,
                UiBuildDate = date,
                UiVersion = uiVersion
            };
        }
    }
}