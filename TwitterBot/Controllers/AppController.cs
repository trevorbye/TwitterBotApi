using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;
using System.Security.Cryptography;
using System.Text;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class AppController : ApiController
    {
        private string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        [Route("twitter-auth-token")]
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
            string authToken = content.Substring(content.IndexOf("oauth_token=") + 12, content.IndexOf("&oauth_token_secret") - 12);
            return Ok(authToken);
        }
    }
}
