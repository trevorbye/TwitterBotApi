using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TwitterBot.Extensions;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class AppController : ApiController
    {
        private readonly TwitterBotContext _databaseContext = new TwitterBotContext();
        static readonly string ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY");
        static readonly string Secret = Environment.GetEnvironmentVariable("SECRET");
        const string Callback = "https://mstwitterapp.azurewebsites.net/add-account-redirect";

        [HttpGet, Route("api/twitter-auth-token")]
        public async Task<IHttpActionResult> GetTwitterOauthString()
        {
            var baseUrl = WebUtility.UrlEncode("https://api.twitter.com/oauth/request_token");
            var oauthCallback = WebUtility.UrlEncode(Callback);
            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");

            var paramString = "oauth_callback=" + oauthCallback + "&" +
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_version=" + version;

            var signatureBaseString = "POST&" + baseUrl + "&" + WebUtility.UrlEncode(paramString);
            var signingKey = Secret + "&";
            var oauthSignature = signatureBaseString.ToSecureHash(signingKey);

            var authString = "OAuth " +
                "oauth_callback=" + "\"" + oauthCallback + "\"" + ", " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.twitter.com/oauth/request_token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                }
            };

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var parsed = HttpUtility.ParseQueryString(content);
            var authToken = parsed["oauth_token"];

            return Ok(authToken);
        }

        [HttpGet, Route("api/convert-to-access-token")]
        public async Task<IHttpActionResult> GetTwitterAccessToken(string token, string verifier)
        {
            var principle = User.GetUsername();

            var baseUrl = WebUtility.UrlEncode("https://api.twitter.com/oauth/access_token");
            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");
            var oauthToken = WebUtility.UrlEncode(token);
            var oauthVerifier = WebUtility.UrlEncode(verifier);

            var paramString =
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_verifier=" + oauthVerifier + "&" +
                "oauth_version=" + version;

            var signatureBaseString = "POST&" + baseUrl + "&" + WebUtility.UrlEncode(paramString);
            var signingKey = Secret + "&" + oauthToken;
            var oauthSignature = signatureBaseString.ToSecureHash(signingKey);

            var authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_verifier=" + "\"" + oauthVerifier + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.twitter.com/oauth/access_token"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                }
            };

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var parsed = HttpUtility.ParseQueryString(content);
            var twitterAccount = new TwitterAccount
            {
                HandleUser = principle,
                TwitterHandle = "@" + parsed["screen_name"],
                TwitterUserId = long.Parse(parsed["user_id"]),
                OauthToken = parsed["oauth_token"],
                OauthSecret = parsed["oauth_token_secret"],
                IsAutoRetweetEnabled = false
            };

            // Check if account has already been added
            var accounts = 
                _databaseContext.TwitterAccounts
                                .Where(table => table.TwitterHandle == twitterAccount.TwitterHandle)
                                .ToList();

            if (accounts.Count == 0)
            {
                _databaseContext.TwitterAccounts.Add(twitterAccount);
                _databaseContext.SaveChanges();

                return Ok(twitterAccount.TwitterHandle);
            }
            else
            {
                return BadRequest(twitterAccount.TwitterHandle);
            }
        }

        [HttpGet, Route("api/enable-auto-tweets")]
        public IHttpActionResult EnableAccountRetweetStatus(string handle)
        {
            var (ownsHandle, account) = EnsurePrincipleOwnsHandle(handle);
            if (!ownsHandle)
            {
                return BadRequest();
            }
            else
            {
                account.IsAutoRetweetEnabled = true;
                _databaseContext.SaveChanges();

                return Ok();
            }
        }

        [HttpGet, Route("api/disable-auto-tweets")]
        public IHttpActionResult DisableAccountRetweetStatus(string handle)
        {
            var (ownsHandle, account) = EnsurePrincipleOwnsHandle(handle);
            if (!ownsHandle)
            {
                return BadRequest();
            }
            else
            {
                account.IsAutoRetweetEnabled = false;
                _databaseContext.SaveChanges();

                return Ok();
            }
        }

        [HttpGet, Route("api/toggle-private-account")]
        public IHttpActionResult TogglePrivateAccount(string handle, bool isPrivate)
        {
            var (ownsHandle, account) = EnsurePrincipleOwnsHandle(handle);
            if (!ownsHandle)
            {
                return BadRequest();
            }
            else
            {
                account.IsPrivateAccount = isPrivate;
                _databaseContext.SaveChanges();

                return Ok();
            }
        }

        [HttpGet, Route("api/toggle-public-schedule")]
        public IHttpActionResult ToggleMakeSchedulePublic(string handle, bool isPublic)
        {
            var (ownsHandle, account) = EnsurePrincipleOwnsHandle(handle);
            if (!ownsHandle)
            {
                return BadRequest();
            }
            else
            {
                account.IsTweetSchedulePublic = isPublic;
                _databaseContext.SaveChanges();

                return Ok();
            }
        }

        [HttpDelete, Route("api/delete-twitter-account")]
        public IHttpActionResult DeleteTwitterAccount(string handle)
        {
            var (ownsHandle, account) = EnsurePrincipleOwnsHandle(handle);
            if (!ownsHandle)
            {
                return BadRequest();
            }
            else
            {
                _databaseContext.TwitterAccounts.Remove(account);
                _databaseContext.SaveChanges();

                return Ok();
            }
        }

        (bool ownsHandle, TwitterAccount account) EnsurePrincipleOwnsHandle(string handle)
        {
            var principle = User.GetUsername();
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(table => table.TwitterHandle == handle);

            return (account.HandleUser == principle, account);
        }

        [HttpGet, Route("api/get-user-twitter-accounts")]
        public IHttpActionResult GetUserTwitterAccounts()
        {
            var principle = User.GetUsername();

            return Ok(
                _databaseContext.TwitterAccounts
                    .Where(table => table.HandleUser == principle)
                    .Select(account => new
                    {
                        account.TwitterHandle,
                        account.IsAutoRetweetEnabled,
                        account.IsPrivateAccount,
                        account.IsTweetSchedulePublic
                    }));
        }
    }
}