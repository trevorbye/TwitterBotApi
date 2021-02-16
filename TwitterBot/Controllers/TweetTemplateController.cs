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

namespace TwitterBot.Controllers
{
    [Authorize]
    public class TweetTemplateController : ApiController
    {

        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        [HttpGet, Route("api/tweet-templates")]
        public IHttpActionResult GetAllTemplates()
        {
            var list = _databaseContext.TweetTemplates.ToList();
            return Ok(list);
        }



        [HttpPost, Route("api/tweet-template")]
        public IHttpActionResult PostNewTemplate(TweetTemplate tweetTemplate)
        {
            var user = User.GetUsername();
            tweetTemplate.TweetUser = user;

            // find twitter account user
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(a => a.TwitterHandle == tweetTemplate.TwitterHandle);
            tweetTemplate.HandleUser = account.HandleUser;

            // populate last few object fields
            tweetTemplate.Created = DateTime.UtcNow;
            tweetTemplate.Modified = DateTime.UtcNow;

            _databaseContext.TweetTemplates.Add(tweetTemplate);
            _databaseContext.SaveChanges();

            tweetTemplate.HandleUser = null;
            return Ok(tweetTemplate);
        }
    }
}

