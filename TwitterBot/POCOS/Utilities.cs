using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;

namespace TwitterBot.POCOS
{
    public class Utilities
    {
        public static bool isMsftInternalTenant(IEnumerable<Claim> claims)
        {
            foreach (Claim claim in claims)
            {
                if (claim.Type == "iss")
                {

                }
            }
            return false;
        }
    }
}