using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Claims;
using TwitterBot.Extensions;
using System.Web.Http.Results;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Net;

namespace TwitterBot.Controllers
{

    public class TweetAutomationController : ApiController
    {
        static readonly string TweetAutomationBearerToken = Environment.GetEnvironmentVariable("TWEET_AUTOMATION_CLIENT_KEY");
        static readonly string AzureFunctionValidateTweetTextKey = Environment.GetEnvironmentVariable("AZUREFUNTION_VALIDATETWEET_KEY");
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        // get all - For Bobby Reed
        [HttpGet, Route("api/tweet-automation-templates")]
        public IHttpActionResult GetAllTemplates()
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != TweetAutomationBearerToken || authToken is null)
            {
                return Unauthorized();
            }

            var list = _databaseContext.TweetTemplates.ToList();
            return Ok(list);
        }

        // insert 1 tweet - For Bobby Reed
        [HttpPost, Route("api/tweet-automation-tweet")]
        public async Task<IHttpActionResult> PostNewTemplate(TweetQueue tweet)
        {
            // validate requestor
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != TweetAutomationBearerToken || authToken is null)
            {
                return Unauthorized();
            }

            // validate tweet body
            var tweetTextToValidate = new TwitterTextValidationRequest
            {
                tweetText = tweet.StatusBody
            };
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(tweetTextToValidate));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");


            // validate body text with custom twitter Azure Function
            var bearer = $"Bearer {AzureFunctionValidateTweetTextKey}";
            var apiClient = new HttpClient();
            var apiRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://twitterfunctionjs.azurewebsites.net/api/validateTweet"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Accept.ToString(), "application/json"},
                    { "x-functions-key", AzureFunctionValidateTweetTextKey}
                },
                Content = httpContent

            };
            var apiResponse = apiClient.SendAsync(apiRequest).Result;
            apiResponse.EnsureSuccessStatusCode();
            var twitterTextValidationContent = await apiResponse.Content.ReadAsAsync<TwitterTextValidationResponse>();
            if (twitterTextValidationContent.isValid == false)
            {
                return BadRequest("Tweet text boby failed twitter validation");
            }
            

            // find twitter handle approver
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(a => a.TwitterHandle == tweet.TwitterHandle);
            tweet.HandleUser = account.HandleUser;

            // populate last few object fields
            tweet.CreatedTime = DateTime.UtcNow;
            tweet.IsApprovedByHandle = false;
            tweet.IsPostedByWebJob = false;
            if(tweet.ScheduledStatusTime.ToString()== "1/1/0001 12:00:00 AM")
            {
                tweet.ScheduledStatusTime = DateTime.UtcNow;
            }

            // save
            _databaseContext.TweetQueues.Add(tweet);
            _databaseContext.SaveChanges();
            
            // send email to handle approver
            NotificationService.SendNotificationToHandle(tweet);
            tweet.HandleUser = null;

            // return tweet back to requestor with id
            return Ok(tweet);
        }
    }
}
