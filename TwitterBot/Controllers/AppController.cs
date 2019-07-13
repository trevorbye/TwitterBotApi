using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;
using System.Security.Cryptography;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class AppController : ApiController
    {
        [Route("twitter-auth-token")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTwitterOauthString()
        {
            string baseUrl = WebUtility.UrlEncode("https://api.twitter.com/oauth/request_token");
            string oauthCallback = WebUtility.UrlEncode("http://localhost:52937/add-account-redirect");
            string oauthConsumerKey = "OmlAUrh2VxAKX6Qp2bYzwxBwI";
            string oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            string sigMethod = "HMAC-SHA1";
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string version = "1.0";

            string paramString = "oauth_callback=" + oauthCallback + "&" +
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method" + sigMethod + "&" +
                "oauth_timestamp" + timestamp + "&" +
                "oauth_version" + version;

            string signatureBaseString = "POST&" + baseUrl + "&" + WebUtility.UrlEncode(paramString);
            string signingKey = "4HmFBAX1w6xuz4onP0lHwq7ANMQ6vswEokIiNnJY6kHuUd51ek" + "&";

            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(signingKey);
            byte[] messageBytes = encoding.GetBytes(signatureBaseString);
            var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            string oauthSignature = Convert.ToBase64String(hashmessage);

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


            return null;
        }
    }
}
