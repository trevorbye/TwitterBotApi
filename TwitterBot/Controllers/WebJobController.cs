using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Claims;

namespace TwitterBot.Controllers
{
    public class WebJobController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();
        private string _bearerToken = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        [Route("api/webjob-fetch-queue")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult FetchTweetQueueAndAccounts()
        {
            string authToken = Request.Headers.Authorization.Parameter;
            if (authToken != _bearerToken || authToken == null)
            {
                return Unauthorized();
            }

            DateTime timeNowUtc = DateTime.UtcNow;

            IList<TweetQueue> tweetQueues = db.TweetQueues
                .Where(table => table.IsApprovedByHandle == true)
                .Where(table => table.IsPostedByWebJob == false)
                .Where(table => table.ScheduledStatusTime <= timeNowUtc)
                .ToList();

            IList<string> uniqueAccounts = new List<string>();
            foreach (TweetQueue queue in tweetQueues) {
                if (!uniqueAccounts.Contains(queue.HandleUser))
                {
                    uniqueAccounts.Add(queue.HandleUser);
                }
            }

            IDictionary<string, TwitterAccount> accountDict = new Dictionary<string, TwitterAccount>();
            foreach (string handle in uniqueAccounts)
            {
                TwitterAccount account = db.TwitterAccounts
                    .Where(x => x.HandleUser == handle).FirstOrDefault();
                accountDict.Add(handle, account);
            }

            TweetQueueAccountReturnEntity returnEntity = new TweetQueueAccountReturnEntity(tweetQueues, accountDict);
            return Ok(returnEntity);
        }

        [Route("api/webjob-mark-complete")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult MarkAsWebJobComplete(int tweetQueueId)
        {
            string authToken = Request.Headers.Authorization.Parameter;
            if (authToken != _bearerToken || authToken == null)
            {
                return Unauthorized();
            }

            TweetQueue tweetQueue = db.TweetQueues.Find(tweetQueueId);
            if (tweetQueue == null)
            {
                return BadRequest();
            }
            tweetQueue.IsPostedByWebJob = true;
            db.SaveChanges();

            return Ok(0);
        }
    }
}
