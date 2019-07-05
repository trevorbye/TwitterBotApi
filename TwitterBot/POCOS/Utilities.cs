using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using TwitterBot.Models;

namespace TwitterBot.POCOS
{
    public static class Utilities
    {
        public static bool IsAdmin(IEnumerable<Claim> claims, TwitterBotContext db)
        {
            string preferredUsername = "";
            foreach (Claim claim in claims)
            {
                if (claim.Type == "preferred_username")
                {
                    preferredUsername = claim.Value;
                    break;
                }
            }
            var admin = db.AdminManagers
                                .Where(admins => admins.User == preferredUsername)
                                .FirstOrDefault();
            if (admin == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string UsernameFromClaims(IEnumerable<Claim> claims)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == "preferred_username")
                {
                    return claim.Value;
                }
            }
            return "";
        }
    }
}