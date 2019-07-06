﻿using System;
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

        [Route("get-user-tweet-queue")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserTweetQueue()
        {
            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;

            
            string user = Utilities.UsernameFromClaims(claims);
            

            //IList<TweetQueue> tweets = db.TweetQueues.Where(table => table.TweetUser == user).ToList();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var tweets = db.TweetQueues.SqlQuery("SELECT * FROM dbo.TweetQueues WHERE TweetUser=@user", new SqlParameter("user", user)).ToList();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);
            return Ok(tweets);
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