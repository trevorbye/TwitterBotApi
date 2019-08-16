using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Claims;
using System.Data.SqlClient;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class TweetQueuesController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();

        [Route("api/get-user-tweet-queue")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserTweetQueue()
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string user = Utilities.UsernameFromClaims(claims);
            IList<TweetQueue> tweets = db.TweetQueues.Where(table => table.TweetUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();
            return Ok(tweets);
        }

        [Route("api/get-handles-tweet-queue")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetHandlesTweetQueue()
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string user = Utilities.UsernameFromClaims(claims);
            IList<TweetQueue> tweets = db.TweetQueues.Where(table => table.HandleUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();
            return Ok(tweets);
        }

        [Route("api/get-distinct-handles")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDistinctHandles()
        {
            IList<string> distinctHandles = db.TwitterAccounts.Select(table => table.TwitterHandle).Distinct().ToList();
            return Ok(distinctHandles);
        }

        [Route("api/approve-or-cancel")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult ApproveOrCancelTweet(int approveById, int cancelById)
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string user = Utilities.UsernameFromClaims(claims);

            int tweetIdCheck = 0;
            string operationType = "";

            if (cancelById == 0)
            {
                tweetIdCheck = approveById;
                operationType = "approve";
            }
            else
            {
                tweetIdCheck = cancelById;
                operationType = "cancel";
            }

            TweetQueue tweetQueue = db.TweetQueues.Find(tweetIdCheck);
            if (tweetQueue.HandleUser != user)
            {
                return BadRequest();
            }

            if (operationType == "approve")
            {
                tweetQueue.IsApprovedByHandle = true;
            }
            else
            {
                tweetQueue.IsApprovedByHandle = false;
            }

            db.SaveChanges();
            return Ok();
        }

        [Route("api/delete-tweet")]
        [System.Web.Http.HttpDelete]
        public IHttpActionResult DeleteTweet(int id)
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string user = Utilities.UsernameFromClaims(claims);

            // find tweet by id, make sure claims user == tweet user
            TweetQueue tweetQueue = db.TweetQueues.Find(id);
            if (tweetQueue.TweetUser == user)
            {
                db.TweetQueues.Remove(tweetQueue);
                db.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("api/post-new-tweet")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostNewTweet(TweetQueue tweetQueue)
        {
            //get user from auth claim
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;
            string user = Utilities.UsernameFromClaims(claims);
            tweetQueue.TweetUser = user;

            // find twitter account user
            TwitterAccount account = db.TwitterAccounts.Where(table => table.TwitterHandle == tweetQueue.TwitterHandle).FirstOrDefault();
            tweetQueue.HandleUser = account.HandleUser;

            // populate last few object fields
            tweetQueue.CreatedTime = DateTime.UtcNow;
            tweetQueue.IsApprovedByHandle = false;
            tweetQueue.IsCanceledByHandle = false;

            db.TweetQueues.Add(tweetQueue);
            db.SaveChanges();

            tweetQueue.HandleUser = null;
            return Ok(tweetQueue);
        }
    }
}