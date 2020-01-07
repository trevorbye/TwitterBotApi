using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MentionWebJob
{
    class WebActions
    {
        static readonly string ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY");
        static readonly string Secret = Environment.GetEnvironmentVariable("SECRET");

        public static string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        public static async Task CheckAccountMentions(WebJobTwitterAccount account, string bearer)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");

            var oauthToken = WebUtility.UrlEncode(account.OauthToken);
            var oauthSecret = WebUtility.UrlEncode(account.OauthSecret);

            // if null, reset to 1 to avoid API error
            var retweetSinceId = account.RetweetSinceId;
            if (retweetSinceId == 0)
            {
                retweetSinceId = 1;
            }
            var baseUrl = "https://api.twitter.com/1.1/statuses/mentions_timeline.json";

            var paramString =
                "include_entities=false&" +
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_version=" + version + "&" +
                "since_id=" + retweetSinceId + "&" +
                "trim_user=true";

            var signatureBaseString =
                $"GET&{WebUtility.UrlEncode(baseUrl)}&{WebUtility.UrlEncode(paramString)}";
            var signingKey = $"{Secret}&{oauthSecret}";
            var oauthSignature = WebActions.ShaHash(signatureBaseString, signingKey);

            var authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{baseUrl}?include_entities=false&since_id={retweetSinceId}&trim_user=true"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                }
            };

            var response = client.SendAsync(request).Result;
            // parse response
            List<JObject> mentionTimeline = JsonConvert.DeserializeObject<List<JObject>>(response.Content.ReadAsStringAsync().Result);

            int counter = 0;
            long newSinceId = 0;
            List<Tuple<long, string>> mentions = new List<Tuple<long, string>>();
            foreach (JObject node in mentionTimeline)
            {
                string text = (string)node["text"];
                long id = (long)node["id"];
                var mention = Tuple.Create(id, text);
                mentions.Add(mention);

                if (counter == 0)
                {
                    newSinceId = id;
                }
                counter++;
            }

            //make api calls to webjob controller
            foreach (Tuple<long, string> mention in mentions)
            {
                string encodedTweet = WebUtility.UrlEncode(mention.Item2);
                var serviceRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://mstwitterbot.azurewebsites.net/api/post-retweet?newSinceId={newSinceId}&accountId={account.Id}&retweetId={mention.Item1}&tweetBody={encodedTweet}"),
                    Method = HttpMethod.Post,
                    Headers =
                    {
                        { HttpRequestHeader.Authorization.ToString(), bearer }
                    }
                };

                await client.SendAsync(serviceRequest);
            }
        }
    }
}