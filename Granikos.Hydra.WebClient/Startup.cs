using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Granikos.NikosTwo.WebClient.Startup))]
namespace Granikos.NikosTwo.WebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
