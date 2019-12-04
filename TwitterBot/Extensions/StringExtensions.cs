using System;
using System.Security.Cryptography;
using System.Text;

namespace TwitterBot.Extensions
{
    public static class StringExtensions
    {
        public static string ToSecureHash(this string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }
    }
}