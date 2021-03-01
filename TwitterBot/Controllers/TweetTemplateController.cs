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

namespace TwitterBot.Controllers
{
    public class TweetTemplateController : ApiController
    {
        static readonly string BobbyReedBearerToken = Environment.GetEnvironmentVariable("BOBBYREED_CLIENT_KEY");
        static readonly string AzureFunctionValidateTweetTextKey = Environment.GetEnvironmentVariable("AZUREFUNTION_VALIDATETWEET_KEY");
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        // get all - For Bobby Reed
        [HttpGet, Route("api/br-tweet-templates")]
        public IHttpActionResult BrGetAllTemplates()
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BobbyReedBearerToken || authToken is null)
            {
                return Unauthorized();
            }

            var list = _databaseContext.TweetTemplates.ToList();
            return Ok(list);
        }

        // insert 1 tweet - For Bobby Reed
        [HttpPost, Route("api/br-tweet-template")]
        public async Task<IHttpActionResult> BrPostNewTemplate(TweetQueue tweet)
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BobbyReedBearerToken || authToken is null)
            {
                return Unauthorized();
            }

            // validate tweet body
            var bearer = $"Bearer {AzureFunctionValidateTweetTextKey}";
            var apiClient = new HttpClient();
            var apiRequest = new HttpRequestMessage
            {
                RequestUri = new Uri($"https://twitterfunctionjs.azurewebsites.net/api/validateTweet"),
                Method = HttpMethod.Post,
                Headers =
                {
                    { System.Net.HttpRequestHeader.Authorization.ToString(), bearer }
                },
                Content = new System.Net.Http.StringContent("{tweetText:\"" + tweet.StatusBody + "\"}", Encoding.UTF8, "application/json")

            };
            var apiResponse = apiClient.SendAsync(apiRequest).Result;
            var validationResult = apiResponse.Content.ReadAsAsync<TwitterTextValidationResponse>().Result;
            if (validationResult.isValid == false)
            {
                return BadRequest("Tweet text failed validation");
            }


            // find twitter handle approver
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(a => a.TwitterHandle == tweet.TwitterHandle);
            tweet.HandleUser = account.HandleUser;

            // populate last few object fields
            tweet.CreatedTime = DateTime.UtcNow;
            tweet.IsApprovedByHandle = false;
            tweet.IsPostedByWebJob = false;

            _databaseContext.TweetQueues.Add(tweet);
            _databaseContext.SaveChanges();

            NotificationService.SendNotificationToHandle(tweet);
            tweet.HandleUser = null;
            return Ok(tweet);
        }

        // get tweets by person who entered/owns template
        [Authorize]
        [HttpGet, Route("api/tweet-templates-by-handle")]
        public IHttpActionResult GetAllTemplatesByHandle(string twitterHandle)
        {
            var user = User.GetUsername();
            IList<TweetTemplate> templates = _databaseContext.TweetTemplates
                .Where(table => table.TwitterHandle == twitterHandle && table.TweetUser==user)
                .OrderByDescending(x => x.Title).ToList();
            return Ok(templates);
        }


        // insert
        [Authorize]
        [HttpPost, Route("api/tweet-template")]
        public IHttpActionResult PostNewTemplate(TweetTemplate tweetTemplate)
        {
            var user = User.GetUsername();
            tweetTemplate.TweetUser = user; // creator of template and cooresponding tweets

            // find twitter handle approver
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(a => a.TwitterHandle == tweetTemplate.TwitterHandle);
            tweetTemplate.HandleUser = account.HandleUser;

            // populate last few object fields
            tweetTemplate.Created = DateTime.UtcNow;
            tweetTemplate.Modified = DateTime.UtcNow;

            _databaseContext.TweetTemplates.Add(tweetTemplate);
            _databaseContext.SaveChanges();

            return Ok(tweetTemplate);
        }

        // update
        [Authorize]
        [HttpPatch, Route("api/tweet-template")]
        public IHttpActionResult PostEditedTemplate(TweetTemplate tweetTemplate)
        {
            var user = User.GetUsername();

            var existingTemplate = _databaseContext.TweetTemplates.Find(tweetTemplate.Id);

            // if not creator or handle approver, then fail
            if ((existingTemplate.HandleUser != user)||(existingTemplate.TweetUser != user))
            {
                return BadRequest();
            }


            existingTemplate.ChangedThresholdPercentage = tweetTemplate.ChangedThresholdPercentage;
            existingTemplate.Channel = tweetTemplate.Channel;
            existingTemplate.CodeChanges = tweetTemplate.CodeChanges;
            existingTemplate.External = tweetTemplate.External;
            existingTemplate.ForceNotifyTag = tweetTemplate.ForceNotifyTag;
            existingTemplate.IgnoreMetadataOnly = tweetTemplate.IgnoreMetadataOnly;
            existingTemplate.Modified = DateTime.UtcNow;
            existingTemplate.NewFiles = tweetTemplate.NewFiles;
            existingTemplate.QueryString = tweetTemplate.QueryString;
            existingTemplate.Rss = tweetTemplate.Rss;
            existingTemplate.SearchBy = tweetTemplate.SearchBy;
            existingTemplate.SearchType = tweetTemplate.SearchType;
            existingTemplate.TemplateText = tweetTemplate.TemplateText;
            existingTemplate.Title = tweetTemplate.Title;
            existingTemplate.TwitterHandle = tweetTemplate.TwitterHandle;

            _databaseContext.SaveChanges();

            return Ok();
        }

        // delete
        [Authorize]
        [HttpDelete, Route("api/tweet-template")]
        public IHttpActionResult DeleteTemplate(int Id)
        {
            var user = User.GetUsername();

            // find tweet by id, make sure claims user == tweet user OR handle user
            var tweetTemplate = _databaseContext.TweetTemplates.Find(Id);
            if (tweetTemplate.TweetUser == user || tweetTemplate.HandleUser == user)
            {
                _databaseContext.TweetTemplates.Remove(tweetTemplate);
                _databaseContext.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}

