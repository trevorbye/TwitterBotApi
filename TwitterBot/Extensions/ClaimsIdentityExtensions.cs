using System.Security.Claims;
using System.Security.Principal;

namespace TwitterBot.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        public static string GetUsername(this IPrincipal user) =>
            user is ClaimsPrincipal claimsUser ? GetUsername(claimsUser) : null;

        static string GetUsername(ClaimsPrincipal claimsUser) =>
            claimsUser?.Identity is ClaimsIdentity claimsId ? GetUsername(claimsId) : null;

        static string GetUsername(ClaimsIdentity claimsId) =>
            claimsId.FindFirst("preferred_username")?.Value;
    }
}