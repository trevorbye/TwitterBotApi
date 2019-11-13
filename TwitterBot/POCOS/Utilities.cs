using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TwitterBot.Models;

namespace TwitterBot.POCOS
{
    public static class Utilities
    {
        public static bool IsAdmin(IEnumerable<Claim> claims, TwitterBotContext databaseContext)
        {
            var preferredUsername = UsernameFromClaims(claims);
            return databaseContext.AdminManagers
                                  .FirstOrDefault(
                admins => admins.User == preferredUsername) != null;
        }

        public static string UsernameFromClaims(IEnumerable<Claim> claims) =>
            claims.FirstOrDefault(claim => claim.Type == "preferred_username")
                  ?.Value ?? "";
    }
}