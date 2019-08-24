using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;

namespace TwitterWebJob
{
    class TestMain
    {
        private static string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        public static void MainTest(String[] args)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            string oauthConsumerKey = WebUtility.UrlEncode("Z56vS7LzR2KlEN44FCGDz6g3g");
            string oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            string sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string version = WebUtility.UrlEncode("1.0");

            // these come from account object
            string oauthToken = WebUtility.UrlEncode("1149458747686735872-tid82ZGH2wMp3HOQd8C3xAhcslLS9Q");
            string oauthSecret = WebUtility.UrlEncode("UKxVUoXN6eprZ9ZWlQlWweoQgBuJinhI5qKbGSUfn5kBT");

            string status = Uri.EscapeDataString("A new article");
            string baseUrl = "https://api.twitter.com/1.1/statuses/update.json";

            string paramString =
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_version=" + version + "&" +
                "status=" + status;

            string signatureBaseString = "POST&" + WebUtility.UrlEncode(baseUrl) + "&" 
                + WebUtility.UrlEncode(paramString);
            string signingKey = "7WXse93IFMia1Lj3VvGh83L1wCFfJqQM4Cj9mkkZClqHNSfs5M" + "&" + oauthSecret;
            string oauthSignature = ShaHash(signatureBaseString, signingKey);

            string authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri(baseUrl + "?status=" + status),
                Method = HttpMethod.Post,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), authString}
                }
            };
            var response = client.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            int bp = 0;
        }
    }
}
