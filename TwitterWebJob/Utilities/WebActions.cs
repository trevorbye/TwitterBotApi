using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitterWebJob
{
    class WebActions
    {
        static readonly string ConsumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY");
        static readonly string Secret = Environment.GetEnvironmentVariable("SECRET");
        static readonly string Token = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        public static string ShaHash(string value, string signingKey)
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

        public static List<long> BuildMediaIds(WebJobTweetQueue tweetQueue,
            string oauthConsumerKey,
            string oauthToken,
            string oauthUserSecret)
        {
            var mediaIds = new List<long>();

            // get image base64 strings from internal api
            var bearer = $"Bearer {Token}";
            var apiClient = new HttpClient();
            var apiRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://mstwitterbot.azurewebsites.net/api/get-image-streams?tweetQueueId={tweetQueue.Id}"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), bearer }
                }
            };

            var apiResponse = apiClient.SendAsync(apiRequest).Result;
            var base64Strings = apiResponse.Content.ReadAsAsync<List<string>>().Result;

            foreach (var base64 in base64Strings)
            {
                var baseUrl = "https://upload.twitter.com/1.1/media/upload.json";
                var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
                var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var version = WebUtility.UrlEncode("1.0");
                
                var paramString =
                    "oauth_consumer_key=" + oauthConsumerKey + "&" +
                    "oauth_nonce=" + oauthNonce + "&" +
                    "oauth_signature_method=" + sigMethod + "&" +
                    "oauth_timestamp=" + timestamp + "&" +
                    "oauth_token=" + oauthToken + "&" +
                    "oauth_version=" + version;

                var signatureBaseString =
                $"POST&{WebUtility.UrlEncode(baseUrl)}&{WebUtility.UrlEncode(paramString)}";
                var signingKey = $"{Secret}&{oauthUserSecret}";
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
                var formContent = new MultipartFormDataContent();
                byte[] imgStream = Convert.FromBase64String(base64);
                formContent.Add(new ByteArrayContent(imgStream), "media");

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(baseUrl),
                    Method = HttpMethod.Post,
                    Headers =
                    {
                        { HttpRequestHeader.Authorization.ToString(), authString },
                        { HttpRequestHeader.ContentType.ToString(), "multipart/form-data" }
                    },
                    Content = formContent
                };
                var response = client.SendAsync(request).Result;
                JObject res = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
                mediaIds.Add((long)res["media_id"]);
            }
            return mediaIds;
        }

        public static async Task SendTweetAsync(
            WebJobTweetQueue tweetQueue,
            IList<WebJobTwitterAccount> accountsList,
            string bearer)
        {
            bool isRetweet = false;
            bool hasImages = false;
            if (tweetQueue.BlockBlobIdsConcat != null)
            {
                hasImages = true;
            }
            if (tweetQueue.RetweetNum != 0) 
            {
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
            string encodedMediaIds = null;
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

                if (hasImages)
                {
                    var mediaIds = BuildMediaIds(tweetQueue, oauthConsumerKey, oauthToken, oauthSecret);
                    var mediaIdString = "";
                    foreach (var id in mediaIds)
                    {
                        mediaIdString += id.ToString();
                        mediaIdString += ",";
                    }
                    mediaIdString = mediaIdString.TrimEnd(',');
                    encodedMediaIds = WebUtility.UrlEncode(mediaIdString);

                    paramString =
                        "media_ids=" + encodedMediaIds + "&" +
                        "oauth_consumer_key=" + oauthConsumerKey + "&" +
                        "oauth_nonce=" + oauthNonce + "&" +
                        "oauth_signature_method=" + sigMethod + "&" +
                        "oauth_timestamp=" + timestamp + "&" +
                        "oauth_token=" + oauthToken + "&" +
                        "oauth_version=" + version + "&" +
                        "status=" + status;
                }
                else
                {
                    paramString =
                        "oauth_consumer_key=" + oauthConsumerKey + "&" +
                        "oauth_nonce=" + oauthNonce + "&" +
                        "oauth_signature_method=" + sigMethod + "&" +
                        "oauth_timestamp=" + timestamp + "&" +
                        "oauth_token=" + oauthToken + "&" +
                        "oauth_version=" + version + "&" +
                        "status=" + status;
                }
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
                if (hasImages) 
                {
                    uri = new Uri($"{baseUrl}?media_ids={encodedMediaIds}&status={status}");
                }
                else
                {
                    uri = new Uri($"{baseUrl}?status={status}");
                }
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