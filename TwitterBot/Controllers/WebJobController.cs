using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    public class WebJobController : ApiController
    {
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();
        static readonly string BearerToken = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        [HttpGet, Route("api/webjob-fetch-queue")]
        public IHttpActionResult FetchTweetQueueAndAccounts()
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BearerToken || authToken is null)
            {
                return Unauthorized();
            }

            var timeNowUtc = DateTime.UtcNow;
            var tweetQueues =
                _databaseContext.TweetQueues
                                .Where(table => table.IsApprovedByHandle)
                                .Where(table => !table.IsPostedByWebJob)
                                .Where(table => table.ScheduledStatusTime <= timeNowUtc)
                                .ToList();

            var uniqueAccounts = new HashSet<string>();
            tweetQueues.ForEach(queue => uniqueAccounts.Add(queue.HandleUser));

            var accountDict =
                uniqueAccounts.Select(handle => _databaseContext.TwitterAccounts.FirstOrDefault(x => x.HandleUser == handle))
                              .ToDictionary(account => account.HandleUser, account => account);

            var returnEntity = new TweetQueueAccountReturnEntity(tweetQueues, accountDict);

            return Ok(returnEntity);
        }

        [HttpGet, Route("api/webjob-mark-complete")]
        public IHttpActionResult MarkAsWebJobComplete(int tweetQueueId)
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BearerToken || authToken == null)
            {
                return Unauthorized();
            }

            var tweetQueue = _databaseContext.TweetQueues.Find(tweetQueueId);
            if (tweetQueue is null)
            {
                return BadRequest();
            }

            tweetQueue.IsPostedByWebJob = true;
            _databaseContext.SaveChanges();

            return Ok(0);
        }
    }
}