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
using System.IO;

namespace TwitterWebJob
{
    class Test
    {
        public static void Main0(string[] args)
        {
            var mediaIds = GetMediaIds();
            var mediaIdString = "";
            foreach (var id in mediaIds)
            {
                mediaIdString += id.ToString();
                mediaIdString += ",";
            }
            mediaIdString = mediaIdString.TrimEnd(',');

            string ConsumerKey = "E4BdX6Shn32xHNX4ydpW1Kj0b";
            string Secret = "g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf";

            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthToken = WebUtility.UrlEncode("1149458747686735872-tid82ZGH2wMp3HOQd8C3xAhcslLS9Q");
            var oauthUserSecret = WebUtility.UrlEncode("UKxVUoXN6eprZ9ZWlQlWweoQgBuJinhI5qKbGSUfn5kBT");

            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");

            string status = Uri.EscapeDataString("testing tweet with images");
            string encodedMediaIds = WebUtility.UrlEncode(mediaIdString);
            string baseUrl = $"https://api.twitter.com/1.1/statuses/update.json";
            
            string paramString =
                "media_ids=" + encodedMediaIds + "&" +
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_version=" + version + "&" +
                "status=" + status;

            var signatureBaseString =
                $"POST&{WebUtility.UrlEncode(baseUrl)}&{WebUtility.UrlEncode(paramString)}";
            var signingKey = $"{Secret}&{oauthUserSecret}";
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
                RequestUri = new Uri(baseUrl + $"?media_ids={encodedMediaIds}&status={status}"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                }
            };

            var response = client.SendAsync(request).Result;
            bool success = response.IsSuccessStatusCode;
            var bp = 0;
        }

        static List<long> GetMediaIds()
        {
            string ConsumerKey = "E4BdX6Shn32xHNX4ydpW1Kj0b";
            string Secret = "g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf";

            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthToken = WebUtility.UrlEncode("1149458747686735872-tid82ZGH2wMp3HOQd8C3xAhcslLS9Q");
            var oauthUserSecret = WebUtility.UrlEncode("UKxVUoXN6eprZ9ZWlQlWweoQgBuJinhI5qKbGSUfn5kBT");

            // TEST START

            var mediaIds = new List<long>();
            // get from api endpoint in prod
            // data:image/png;base64,
            string Token = "efa90c3c-586e-48f0-8a7f-0cc188726d50-990rtfgjs";
            var bearer = $"Bearer {Token}";
            var apiClient = new HttpClient();
            var apiRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://mstwitterbot.azurewebsites.net/api/get-image-streams?tweetQueueId=212"),
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

                var encodedData = WebUtility.UrlEncode(base64);
                var paramString =
                    //"media_data=" + encodedData + "&" +
                    "oauth_consumer_key=" + oauthConsumerKey + "&" +
                    "oauth_nonce=" + oauthNonce + "&" +
                    "oauth_signature_method=" + sigMethod + "&" +
                    "oauth_timestamp=" + timestamp + "&" +
                    "oauth_token=" + oauthToken + "&" +
                    "oauth_version=" + version;

                var signatureBaseString =
                $"POST&{WebUtility.UrlEncode(baseUrl)}&{WebUtility.UrlEncode(paramString)}";
                var signingKey = $"{Secret}&{oauthUserSecret}";
                var oauthSignature = WebActions.ShaHash(signatureBaseString, signingKey);

                var authString = "OAuth " +
                    "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                    "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                    "oauth_signature=" + "\"" + WebUtility.UrlEncode(oauthSignature) + "\"" + ", " +
                    "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                    "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                    "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                    "oauth_version=" + "\"" + version + "\"";

                var body = new Dictionary<string, string>();
                body.Add("media_data", base64);
                var client = new HttpClient();

                var formContent = new MultipartFormDataContent();
                byte [] imgStream = Convert.FromBase64String(base64);
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
    }
}
