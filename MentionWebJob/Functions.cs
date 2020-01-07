using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System;
using System.Net;
using System.Net.Http;

namespace MentionWebJob
{
    public class Functions
    {
        static readonly string Token = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        [NoAutomaticTrigger]
        public static void ProcessRetweets(TextWriter log)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var bearer = $"Bearer {Token}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/mention-webjob-fetch-accounts"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), bearer }
                }
            };

            var response = client.SendAsync(request).Result;
            var uniqueAccounts = response.Content.ReadAsAsync<List<WebJobTwitterAccount>>().Result;

            if (uniqueAccounts is null)
            {
                return;
            }

            List<Task> asyncCalls = new List<Task>();
            foreach (WebJobTwitterAccount account in uniqueAccounts)
            {
                asyncCalls.Add(WebActions.CheckAccountMentions(account, bearer));
            }

            Task.WaitAll(asyncCalls.ToArray());
        }
    }
}
