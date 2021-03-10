using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;


namespace TwitterBot.App_Start
{
    public class CustomAuthProvider : OAuthBearerAuthenticationProvider
    {
        public override Task ValidateIdentity(OAuthValidateIdentityContext context)
        {
            List<Claim> claims = context.Ticket.Identity.Claims.ToList();
            var userName = claims[9].Value;
            if (!userName.EndsWith("@microsoft.com"))
            {
                context.Rejected();
            }
            return base.ValidateIdentity(context);
        }
    }
}