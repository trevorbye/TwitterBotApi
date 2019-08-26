﻿using System;
using System.Collections.Generic;
using System.IO;
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
            string bearer = "Bearer 893871c2-65cf-4677-a846-435fe9e5f321";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/webjob-fetch-queue"),
                Method = HttpMethod.Get,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), bearer}
                }
            };

            var response = client.SendAsync(request).Result;
            WebJobTweetQueueAccountReturnEntity content = response.Content
                .ReadAsAsync<WebJobTweetQueueAccountReturnEntity>().Result;

            IDictionary<string, WebJobTwitterAccount> accountsDict = content.Accounts;
            List<Task> asyncCalls = new List<Task>();
           
            foreach (WebJobTweetQueue tweetQueue in content.Tweets)
            {
                asyncCalls.Add(WebActions.SendTweet(tweetQueue, accountsDict, bearer));
            }

            Task.WaitAll(asyncCalls.ToArray());
        }
    }
}