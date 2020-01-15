﻿using System;
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
        static readonly string ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY");
        static readonly string Secret = Environment.GetEnvironmentVariable("SECRET");

        static string ShaHash(string value, string signingKey)
        {
            using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(value)));
            }
        }

        static WebJobTwitterAccount getAccountFromListByHandle(string handle, IList<WebJobTwitterAccount> accountsList)
        {
            foreach (WebJobTwitterAccount acct in accountsList)
            {
                if (acct.TwitterHandle == handle)
                {
                    return acct;
                }
            }
            return null;
        }

        public static async Task SendTweetAsync(
            WebJobTweetQueue tweetQueue,
            IList<WebJobTwitterAccount> accountsList,
            string bearer)
        {
            bool isRetweet = false;
            if (tweetQueue.RetweetNum != 0) {
                isRetweet = true;
            }
            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");
            var webJobTwitterAccount = getAccountFromListByHandle(tweetQueue.TwitterHandle, accountsList);
            var oauthToken = WebUtility.UrlEncode(webJobTwitterAccount.OauthToken);
            var oauthSecret = WebUtility.UrlEncode(webJobTwitterAccount.OauthSecret);

            string baseUrl;
            string paramString;
            string status;
            if (isRetweet)
            {
                baseUrl = $"https://api.twitter.com/1.1/statuses/retweet/{tweetQueue.RetweetNum}.json";
                status = "";
                paramString =
                    "oauth_consumer_key=" + oauthConsumerKey + "&" +
                    "oauth_nonce=" + oauthNonce + "&" +
                    "oauth_signature_method=" + sigMethod + "&" +
                    "oauth_timestamp=" + timestamp + "&" +
                    "oauth_token=" + oauthToken + "&" +
                    "oauth_version=" + version;
            }
            else 
            {
                status = Uri.EscapeDataString(tweetQueue.StatusBody);
                baseUrl = "https://api.twitter.com/1.1/statuses/update.json";

                paramString =
                    "oauth_consumer_key=" + oauthConsumerKey + "&" +
                    "oauth_nonce=" + oauthNonce + "&" +
                    "oauth_signature_method=" + sigMethod + "&" +
                    "oauth_timestamp=" + timestamp + "&" +
                    "oauth_token=" + oauthToken + "&" +
                    "oauth_version=" + version + "&" +
                    "status=" + status;
            }
            
            var signatureBaseString =
                $"POST&{WebUtility.UrlEncode(baseUrl)}&{WebUtility.UrlEncode(paramString)}";
            var signingKey = $"{Secret}&{oauthSecret}";
            var oauthSignature = ShaHash(signatureBaseString, signingKey);

            var authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            var client = new HttpClient();
            Uri uri;
            if (isRetweet)
            {
                uri = new Uri(baseUrl);
            }
            else
            {
                uri = new Uri($"{baseUrl}?status={status}");
            }

            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                }
            };
            
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var serviceRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://mstwitterbot.azurewebsites.net/api/webjob-mark-complete?tweetQueueId={tweetQueue.Id}"),
                    Method = HttpMethod.Get,
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