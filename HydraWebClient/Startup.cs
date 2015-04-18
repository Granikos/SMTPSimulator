using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HydraWebClient.Startup))]
namespace HydraWebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
