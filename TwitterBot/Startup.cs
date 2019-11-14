using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TwitterBot.Startup))]
namespace TwitterBot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app) => ConfigureAuth(app);
    }
}