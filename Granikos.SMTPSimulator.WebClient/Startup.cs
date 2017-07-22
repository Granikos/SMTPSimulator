using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Granikos.SMTPSimulator.WebClient.Startup))]
namespace Granikos.SMTPSimulator.WebClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
