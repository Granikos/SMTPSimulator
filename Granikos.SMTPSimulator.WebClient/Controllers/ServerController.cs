using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Granikos.SMTPSimulator.Service.ConfigurationService.Models;

namespace Granikos.SMTPSimulator.WebClient.Controllers
{
    // [Authorize]
    [RoutePrefix("api/Server")]
    public class ServerController : ApiController
    {
        readonly ConfigurationServiceClient _service = new ConfigurationServiceClient();

        [HttpGet]
        [Route("Start")]
        public void Start()
        {
            _service.Start();
        }

        [HttpGet]
        [Route("Stop")]
        public void Stop()
        {
            _service.Stop();
        }

        [HttpGet]
        [Route("IsRunning")]
        public bool IsRunning()
        {
            return _service.IsRunning();
        }

        [HttpGet]
        [Route("Version")]
        public IEnumerable<VersionInfo> GetVersion()
        {
            var versions = _service.GetVersionInfo();
            var assembly = typeof(ServerController).Assembly;

            return versions.Concat(new[]
            { new VersionInfo
                {
                    Assembly = assembly.GetName().Name.Split('.').Last(),
                    Version = assembly.GetName().Version,
                    BuildDate = assembly.GetBuildDate()
                }
            });
        }
    }
}