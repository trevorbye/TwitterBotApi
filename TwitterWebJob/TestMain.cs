using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace TwitterWebJob
{
    class TestMain
    {
        public static void Main0(String[] args)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            string authString = "Bearer ";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/webjob-fetch-queue"),
                Method = HttpMethod.Get,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), authString}
                }
            };

            var response = client.SendAsync(request).Result;
            WebJobTweetQueueAccountReturnEntity content = response.Content
                .ReadAsAsync<WebJobTweetQueueAccountReturnEntity>().Result;

            foreach (WebJobTweetQueue tweetQueue in content.Tweets)
            {

            }

        }
    }
}
