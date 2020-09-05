using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;



namespace TwitterWebJob
{
    class Test
    {
        static string PercentEncode3986(string input)
        {
            // List containing all byte values that don't need to be escaped.
            // Source: https://dev.twitter.com/docs/auth/percent-encoding-parameters
            byte[] nonEscaped =
            {
                // Digits
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,

                // Uppercase Letters
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D,
                0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,

                // Lowercase Letters
                0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D,
                0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A,

                // Reserved Characters
                0x2D, 0x2E, 0x5F, 0x7E
            };

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            List<byte> output = new List<byte>();

            foreach (byte b in bytes)
            {
                if (nonEscaped.Contains(b))
                {
                    output.Add(b);
                }
                else
                {
                    // add percent char
                    output.Add(0x25);

                    // encode first and last half of byte
                    int first = b & 0x0F;
                    int last = b >> 4;
                    output.Add(Convert.ToByte(last.ToString("X")[0]));
                    output.Add(Convert.ToByte(first.ToString("X")[0]));
                }
            }

            return Encoding.UTF8.GetString(output.ToArray());
        }

        static Uri ForceDoubleEncodeSafeChars(string url)
        {
            var encodingReplaceMap = new Dictionary<string, string> 
            {
                { "%2A", "%252A" },
                { "%21", "!" },
                { "%29", "%2529" },
                { "%28", "%2528" },
                { "%2E", "%252E" },
                { "%5F", "%255F" },
                { "%2D", "%252D" },
            };

            var uri = new Uri(url);
            if (uri.AbsoluteUri != uri.OriginalString)
            {
                foreach (KeyValuePair<string, string> entry in encodingReplaceMap)
                {
                    url = url.Replace(entry.Key, entry.Value);
                }
                return new Uri(url);
            } 
            return uri;
        }

        public static void Main1(string[] args)
        {
            string ConsumerKey = "E4BdX6Shn32xHNX4ydpW1Kj0b";
            string Secret = "g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf";
            var oauthToken = WebUtility.UrlEncode("1149458747686735872-tid82ZGH2wMp3HOQd8C3xAhcslLS9Q");
            var oauthSecret = WebUtility.UrlEncode("UKxVUoXN6eprZ9ZWlQlWweoQgBuJinhI5qKbGSUfn5kBT");

            var oauthConsumerKey = WebUtility.UrlEncode(ConsumerKey);
            var oauthNonce = WebUtility.UrlEncode(Guid.NewGuid().ToString("N"));
            var sigMethod = WebUtility.UrlEncode("HMAC-SHA1");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var version = WebUtility.UrlEncode("1.0");
            

            string baseUrl;
            string paramString;
            string status;
            string encodedMediaIds = null;

            var testStatus = "📣  The @dotnet docs team and the amazing #DeveloperCommunity cranked out many new articles and updates in August!" +
                             "\n \n" +
                            "🔥  See \"What's new for August 2020\":" + "\n" +
                            "https://docs.microsoft.com/dotnet/whats-new/2020-08?WT.mc_id=dapine" +
                            "\n \n" +
                            "⌨️ / cc @davidpine7 @billwagner @AndyDeGe @camsoper @screechowl25 @BritchDavid" +
                            "\n \n" +
                            "#DotNetThursday";
            var testStatus2 = "📣 The dotnet docs team and the amazing DeveloperCommunity cranked out many new articles and updates in August!";
            var basicStatus = "🔥 could it be working!";
            var emojiWithText = "📣 does not work!";

            status = PercentEncode3986(basicStatus);
            Console.WriteLine(status);

            baseUrl = "https://api.twitter.com/1.1/statuses/update.json";
            paramString =
                "oauth_consumer_key=" + oauthConsumerKey + "&" +
                "oauth_nonce=" + oauthNonce + "&" +
                "oauth_signature_method=" + sigMethod + "&" +
                "oauth_timestamp=" + timestamp + "&" +
                "oauth_token=" + oauthToken + "&" +
                "oauth_version=" + version + "&" +
                "status=" + status;
                
            var signatureBaseString =
                $"POST&{WebUtility.UrlEncode(baseUrl)}&{PercentEncode3986(paramString)}";
            var signingKey = $"{Secret}&{oauthSecret}";
            var oauthSignature = WebActions.ShaHash(signatureBaseString, signingKey);

            var authString = "OAuth " +
                "oauth_consumer_key=" + "\"" + oauthConsumerKey + "\"" + ", " +
                "oauth_nonce=" + "\"" + oauthNonce + "\"" + ", " +
                "oauth_signature=" + "\"" + PercentEncode3986(oauthSignature) + "\"" + ", " +
                "oauth_signature_method=" + "\"" + sigMethod + "\"" + ", " +
                "oauth_timestamp=" + "\"" + timestamp + "\"" + ", " +
                "oauth_token=" + "\"" + oauthToken + "\"" + ", " +
                "oauth_version=" + "\"" + version + "\"";

            var client = new HttpClient();
            Uri uri = new Uri($"{baseUrl}");
            var testStr = $"status={status}";


            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), authString }
                },
                Content = new StringContent($"status={status}", Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            

            var response = client.SendAsync(request).Result;
            var bp = 0;
        }
    

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
