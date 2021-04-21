using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TwitterBot.Extensions;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class TweetTemplateController : ApiController
    {
        static readonly string TweetAutomationUriWithCode = Environment.GetEnvironmentVariable("TWEET_AUTOMATION_URI_WITH_CODE");
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();
        readonly static HttpClient _httpCient = new HttpClient();

        // get tweets by person who entered/owns template
        [HttpGet, Route("api/tweet-templates-by-handle")]
        public IHttpActionResult GetAllTemplatesByHandle(string twitterHandle)
        {
            try
            {
                var user = User.GetUsername();
                IList<TweetTemplate> templates = _databaseContext.TweetTemplates
                    .Where(table => table.TwitterHandle == twitterHandle && table.TweetUser == user)
                    .OrderByDescending(x => x.Title).ToList();
                return Ok(templates);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Trace.TraceError(e.Message.ToString());
                return BadRequest(e.Message.ToString());
            }
        }

        // insert
        [HttpPost, Route("api/tweet-template")]
        public async Task<IHttpActionResult> PostNewTemplate(TweetTemplate tweetTemplate)
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

            await NotifyTemplateAutomationSystem(tweetTemplate, "create");

            return Ok(tweetTemplate);
        }

        // update
        [HttpPatch, Route("api/tweet-template")]
        public async Task<IHttpActionResult> PostEditedTemplate(TweetTemplate tweetTemplate)
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

            await NotifyTemplateAutomationSystem(existingTemplate, "update");

            return Ok();
        }

        // delete
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
        public async Task NotifyTemplateAutomationSystem(TweetTemplate tweetTemplate, String action)
        {

            TemplateNotification payLoad = new TemplateNotification
            {
                template = tweetTemplate,
                action = action
            };

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(payLoad));
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var apiRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(TweetAutomationUriWithCode),
                Method = HttpMethod.Post,
                Headers =
                {
                    { HttpRequestHeader.Accept.ToString(), "application/json"}
 
                },
                Content = httpContent

            };
            // Bobby's DocIndex system may return 500s until is understands the payload.
            var apiResponse = await _httpCient.SendAsync(apiRequest);

            return;
        }

    }
}

