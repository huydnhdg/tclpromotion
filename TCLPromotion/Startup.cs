using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TCLPromotion.Startup))]
namespace TCLPromotion
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
