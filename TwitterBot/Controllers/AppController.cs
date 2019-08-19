using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Claims;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class AppController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();

        private string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        [Route("api/twitter-auth-token")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTwitterOauthString()
        {
            string baseUrl = WebUtility.UrlEncode("https://api.twitter.com/oauth/request_token");
            string oauthCallback = WebUtility.UrlEncode("http://localhost:52937/add-account-redirect");
            string oauthConsumerKey = WebUtility.UrlEncode("OmlAUrh2VxAKX6Qp2bYzwxBwI");
            string oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            string sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string version = WebUtility.UrlEncode("1.0");

            string paramString = "oauth_callback=" + oauthCallback + "&" +
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_version=" + version;

            string signatureBaseString = "POST&" + baseUrl + "&" + WebUtility.UrlEncode(paramString);
            string signingKey = "4HmFBAX1w6xuz4onP0lHwq7ANMQ6vswEokIiNnJY6kHuUd51ek" + "&";
            string oauthSignature = ShaHash(signatureBaseString, signingKey);

            string authString = "OAuth " +
                "oauth_callback=" + "\"" + oauthCallback + "\"" + ", " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.twitter.com/oauth/request_token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), authString}
                }
            };
            var response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var parsed = HttpUtility.ParseQueryString(content);
            string authToken = parsed["oauth_token"];
            return Ok(authToken);
        }

        [Route("api/convert-to-access-token")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTwitterAccessToken(string token, string verifier)
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string principle = Utilities.UsernameFromClaims(claims);

            string baseUrl = WebUtility.UrlEncode("https://api.twitter.com/oauth/access_token");
            string oauthConsumerKey = WebUtility.UrlEncode("OmlAUrh2VxAKX6Qp2bYzwxBwI");
            string oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            string sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string version = WebUtility.UrlEncode("1.0");
            string oauthToken = WebUtility.UrlEncode(token);
            string oauthVerifier = WebUtility.UrlEncode(verifier);

            string paramString =
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_verifier=" + oauthVerifier + "&" +
                "oauth_version=" + version;

            string signatureBaseString = "POST&" + baseUrl + "&" + WebUtility.UrlEncode(paramString);
            string signingKey = "4HmFBAX1w6xuz4onP0lHwq7ANMQ6vswEokIiNnJY6kHuUd51ek" + "&" + oauthToken;
            string oauthSignature = ShaHash(signatureBaseString, signingKey);

            string authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_verifier=" + "\"" + oauthVerifier + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.twitter.com/oauth/access_token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), authString}
                }
            };
            var response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var parsed = HttpUtility.ParseQueryString(content);

            TwitterAccount twitterAccount = new TwitterAccount
            {
                HandleUser = principle,
                TwitterHandle = "@" + parsed["screen_name"],
                TwitterUserId = long.Parse(parsed["user_id"]),
                OauthToken = parsed["oauth_token"],
                OauthSecret = parsed["oauth_token_secret"],
                IsAutoRetweetEnabled = false
            };

            //check if account has already been added
            IList<TwitterAccount> accounts = db.TwitterAccounts.Where(
                table => table.TwitterHandle == twitterAccount.TwitterHandle).ToList();

            if (accounts.Count == 0)
            {
                db.TwitterAccounts.Add(twitterAccount);
                db.SaveChanges();
                return Ok(twitterAccount.TwitterHandle);
            }
            else
            {
                return BadRequest(twitterAccount.TwitterHandle);
            }
        }

        [Route("api/enable-auto-tweets")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult EnableAccountRetweetStatus(string handle) {

            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string principle = Utilities.UsernameFromClaims(claims);

            //ensure principle owns the handle
            TwitterAccount account = db.TwitterAccounts.Where(table => table.TwitterHandle == handle)
                .FirstOrDefault();
            if (account.HandleUser != principle)
            {
                return BadRequest();
            }
            else
            {
                account.IsAutoRetweetEnabled = true;
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("api/disable-auto-tweets")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult DisableAccountRetweetStatus(string handle)
        {

            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string principle = Utilities.UsernameFromClaims(claims);

            //ensure principle owns the handle
            TwitterAccount account = db.TwitterAccounts.Where(table => table.TwitterHandle == handle)
                .FirstOrDefault();
            if (account.HandleUser != principle)
            {
                return BadRequest();
            }
            else
            {
                account.IsAutoRetweetEnabled = false;
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("api/delete-twitter-account")]
        [System.Web.Http.HttpDelete]
        public IHttpActionResult DeleteTwitterAccount(string handle)
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string principle = Utilities.UsernameFromClaims(claims);

            //ensure principle owns the handle
            TwitterAccount account = db.TwitterAccounts.Where(table => table.TwitterHandle == handle)
                .FirstOrDefault();
            if (account.HandleUser != principle)
            {
                return BadRequest();
            }
            else
            {
                db.TwitterAccounts.Remove(account);
                db.SaveChanges();
                return Ok();
            }
        }

        [Route("api/get-user-twitter-accounts")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserTwitterAccounts()
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string principle = Utilities.UsernameFromClaims(claims);

            IList<TwitterAccount> accounts = db.TwitterAccounts.Where(
                table => table.HandleUser == principle).ToList();

            IList<TwitterAccount> returnAccounts = new List<TwitterAccount>();
            foreach (TwitterAccount account in accounts)
            {
                TwitterAccount returnAccount = new TwitterAccount();
                returnAccount.TwitterHandle = account.TwitterHandle;
                returnAccount.IsAutoRetweetEnabled = account.IsAutoRetweetEnabled;
                returnAccounts.Add(returnAccount);
            }
            return Ok(returnAccounts);
        }
    }
}
