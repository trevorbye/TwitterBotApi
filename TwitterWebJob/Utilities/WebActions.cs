using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;

namespace TwitterWebJob
{
    class WebActions
    {
        private static string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        public static async Task SendTweet(WebJobTweetQueue tweetQueue, IDictionary<string, WebJobTwitterAccount> accountsDict, string bearer)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            // find correct account in dict
            WebJobTwitterAccount webJobTwitterAccount = accountsDict[tweetQueue.HandleUser];

            string oauthConsumerKey = WebUtility.UrlEncode("Z56vS7LzR2KlEN44FCGDz6g3g");
            string oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            string sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string version = WebUtility.UrlEncode("1.0");

            // from account object
            string oauthToken = WebUtility.UrlEncode(webJobTwitterAccount.OauthToken);
            string oauthSecret = WebUtility.UrlEncode(webJobTwitterAccount.OauthSecret);

            string status = Uri.EscapeDataString(tweetQueue.StatusBody);
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

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpClient markTweetClient = new HttpClient();
                HttpRequestMessage serviceRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/webjob-mark-complete?tweetQueueId=" + tweetQueue.Id.ToString()),
                    Method = HttpMethod.Get,
                    Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), bearer}
                }
                };

                await markTweetClient.SendAsync(serviceRequest);
            }
            else
            {
                return;
            }
        }
    }
}
