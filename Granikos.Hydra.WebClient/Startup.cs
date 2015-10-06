using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Granikos.Hydra.WebClient.Startup))]
namespace Granikos.Hydra.WebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
