using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace TwitterWebJob
{
    class WebActions
    {
        public static async Task SetApproved(int tweetQueueId)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            string authString = "Bearer ";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://mstwitterbot.azurewebsites.net/api/webjob-mark-complete?tweetQueueId=" + tweetQueueId.ToString()),
                Method = HttpMethod.Get,
                Headers =
                {
                    {HttpRequestHeader.Authorization.ToString(), authString}
                }
            };

            await client.SendAsync(request);
        }

    }
}
