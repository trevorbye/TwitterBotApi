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
            IList<TweetQueue> tweets = db.TweetQueues.Where(table => table.TweetUser == user).ToList();
            return Ok(tweets);
        }

        [Route("api/get-distinct-handles")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetDistinctHandles()
        {
            IList<string> distinctHandles = db.TwitterAccounts.Select(table => table.TwitterHandle).Distinct().ToList();
            return Ok(distinctHandles);
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

        // GET: api/TweetQueues/5
        [ResponseType(typeof(TweetQueue))]
        public async Task<IHttpActionResult> GetTweetQueue(int id)
        {
            TweetQueue tweetQueue = await db.TweetQueues.FindAsync(id);
            if (tweetQueue == null)
            {
                return NotFound();
            }

            return Ok(tweetQueue);
        }

        // PUT: api/TweetQueues/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTweetQueue(int id, TweetQueue tweetQueue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tweetQueue.Id)
            {
                return BadRequest();
            }

            db.Entry(tweetQueue).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TweetQueueExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/TweetQueues
        [ResponseType(typeof(TweetQueue))]
        public async Task<IHttpActionResult> PostTweetQueue(TweetQueue tweetQueue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.TweetQueues.Add(tweetQueue);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = tweetQueue.Id }, tweetQueue);
        }

        // DELETE: api/TweetQueues/5
        [ResponseType(typeof(TweetQueue))]
        public async Task<IHttpActionResult> DeleteTweetQueue(int id)
        {
            TweetQueue tweetQueue = await db.TweetQueues.FindAsync(id);
            if (tweetQueue == null)
            {
                return NotFound();
            }

            db.TweetQueues.Remove(tweetQueue);
            await db.SaveChangesAsync();

            return Ok(tweetQueue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TweetQueueExists(int id)
        {
            return db.TweetQueues.Count(e => e.Id == id) > 0;
        }
    }
}