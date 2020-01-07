using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MentionWebJob
{
    class Test
    {
        public static void Main0(string[] args)
        {
            string ConsumerKey = "E4BdX6Shn32xHNX4ydpW1Kj0b";
            string Secret = "g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf";


            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");

            var oauthToken = WebUtility.UrlEncode("1149458747686735872-tid82ZGH2wMp3HOQd8C3xAhcslLS9Q");
            var oauthSecret = WebUtility.UrlEncode("UKxVUoXN6eprZ9ZWlQlWweoQgBuJinhI5qKbGSUfn5kBT");
            var retweetSinceId = 1;
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

            // replace these in real impl
            int accountId = 10;
            string bearer = "Bearer efa90c3c-586e-48f0-8a7f-0cc188726d50-990rtfgjs";

            //make api calls to webjob controller
            foreach (Tuple<long, string> mention in mentions)
            {
                string encodedTweet = WebUtility.UrlEncode(mention.Item2);
                var serviceRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"http://localhost:52937/api/post-retweet?newSinceId={newSinceId}&accountId={accountId}&retweetId={mention.Item1}&tweetBody={encodedTweet}"),
                    Method = HttpMethod.Post,
                    Headers =
                    {
                        { HttpRequestHeader.Authorization.ToString(), bearer }
                    }
                };

                var req =  client.SendAsync(serviceRequest).Result;
            }
            
        }
    }
}
