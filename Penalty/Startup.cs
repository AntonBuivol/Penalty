using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Penalty.Startup))]
namespace Penalty
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
