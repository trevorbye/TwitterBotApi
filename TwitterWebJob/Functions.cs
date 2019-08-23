using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using System.Net;

namespace TwitterWebJob
{
    public class Functions
    {
        [NoAutomaticTrigger]
        public static void ProcessTweets(TextWriter log)
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

            List<Task> asyncCalls = new List<Task>();
            foreach (WebJobTweetQueue tweetQueue in content.Tweets)
            {
                asyncCalls.Add(WebActions.SetApproved(tweetQueue.Id));
            }

            Task.WaitAll(asyncCalls.ToArray());
        }
    }
}
