using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;

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

            var accounts = _databaseContext.TwitterAccounts.ToList();
            var returnEntity = new TweetQueueAccountReturnEntity(tweetQueues, accounts);
            return Ok(returnEntity);
        }

        [HttpGet, Route("api/mention-webjob-fetch-accounts")]
        public IHttpActionResult FetchUniqueAccounts()
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BearerToken || authToken is null)
            {
                return Unauthorized();
            }

            var uniqueAccounts = _databaseContext.TwitterAccounts.Where(table => table.IsAutoRetweetEnabled).ToList();
            return Ok(uniqueAccounts);
        }

        [HttpPost, Route("api/post-retweet")]
        public IHttpActionResult PostRetweet(long newSinceId, int accountId, long retweetId, string tweetBody)
        {
            var authToken = Request.Headers.Authorization.Parameter;
            if (authToken != BearerToken || authToken is null)
            {
                return Unauthorized();
            }

            var account = _databaseContext.TwitterAccounts.Find(accountId);
            account.RetweetSinceId = newSinceId;

            // build new tweetqueue object
            TweetQueue tweet = new TweetQueue();
            tweet.HandleUser = account.HandleUser;
            tweet.StatusBody = tweetBody;
            tweet.TwitterHandle = account.TwitterHandle;
            tweet.TweetUser = "AutoRetweetService";
            tweet.RetweetNum = retweetId;

            tweet.CreatedTime = DateTime.UtcNow;
            tweet.ScheduledStatusTime = DateTime.UtcNow;
            tweet.IsApprovedByHandle = false;
            tweet.IsPostedByWebJob = false;

            _databaseContext.TweetQueues.Add(tweet);
            _databaseContext.SaveChanges();

            NotificationService.SendNotificationToHandle(tweet);
            return Ok();
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

        [HttpGet, Route("api/get-image-streams")]
        public IHttpActionResult GetImageStreams(int tweetQueueId)
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

            BlockBlobManager manager = new BlockBlobManager();
            return Ok(manager.DownloadBase64FileStringsNoMime(tweetQueue.GetBlockBlobIdsAsList()));
        }
    }
}