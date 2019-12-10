using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;

namespace TwitterBot
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            var tvps = new TokenValidationParameters
            {
                ValidAudience = ConfigurationManager.AppSettings["ida:Audience"],
                ValidIssuer = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0"
            };
            
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = 
                    new JwtFormat(
                        tvps, 
                        new OpenIdConnectCachingSecurityTokenProvider(
                            "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration")),
            });
        }
    }
}