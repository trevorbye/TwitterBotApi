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
    [Authorize]
    public class TweetTemplateController : ApiController
    {
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        [HttpGet, Route("api/tweet-templates-all")]
        public IHttpActionResult GetAllTemplates()
        {
            var list = _databaseContext.TweetTemplates.ToList();
            return Ok(list);
        }



        // get tweets by person who entered/owns template
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

