using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using System.Net;
using System.Linq;

namespace TwitterWebJob
{
    public class Functions
    {
        static readonly string Token = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        [NoAutomaticTrigger]
        public static async Task ProcessTweets(TextWriter log)
        {
            var bearer = $"Bearer {Token}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/webjob-fetch-queue"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), bearer }
                }
            };

            var response = await client.SendAsync(request);
            var content =
                await response.Content
                              .ReadAsAsync<WebJobTweetQueueAccountReturnEntity>();

            var accountsList = content?.Accounts;
            if (accountsList is null)
            {
                return;
            }

            await Task.WhenAll(
                content.Tweets
                       .Select(
                    tweetQueue => 
                        WebActions.SendTweetAsync(tweetQueue, accountsList, bearer)));
        }
    }
}