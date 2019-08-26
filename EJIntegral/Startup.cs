using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EJIntegral.Startup))]
namespace EJIntegral
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
