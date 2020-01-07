using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Claims;
using TwitterBot.Extensions;

namespace TwitterBot.Controllers
{
    [Authorize]
    public class TweetQueuesController : ApiController
    {
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        [HttpGet, Route("api/get-user-tweet-queue")]
        public IHttpActionResult GetUserTweetQueue()
        {
            var user = User.GetUsername();
            IList<TweetQueue> tweets = _databaseContext.TweetQueues.Where(table => table.TweetUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();
            return Ok(tweets);
        }

        [HttpGet, Route("api/get-handles-tweet-queue")]
        public IHttpActionResult GetHandlesTweetQueue()
        {
            var user = User.GetUsername();
            IList<TweetQueue> tweets = _databaseContext.TweetQueues.Where(table => table.HandleUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();
            return Ok(tweets);
        }

        [HttpGet, Route("api/get-distinct-handles")]
        public IHttpActionResult GetDistinctHandles()
        {
            var user = User.GetUsername();

            var handles =
                _databaseContext.TwitterAccounts
                    .Where(account => !account.IsPrivateAccount || account.HandleUser == user)
                    .Select(table => table.TwitterHandle)
                    .Distinct()
                    .ToList();

            return Ok(handles);
        }

        [HttpPost, Route("api/edit-tweet-body")]
        public IHttpActionResult EditTweetByHandleOwner(TweetQueue model)
        {
            var user = User.GetUsername();

            var tweetQueue = _databaseContext.TweetQueues.Find(model.Id);
            var originalStatus = tweetQueue.StatusBody;
            if (tweetQueue.HandleUser != user)
            {
                return BadRequest();
            }

            tweetQueue.StatusBody = model.StatusBody;
            _databaseContext.SaveChanges();
            NotificationService.SendEditNotif(tweetQueue, originalStatus);
        
            return Ok();
        }

        [HttpGet, Route("api/approve-or-cancel")]
        public IHttpActionResult ApproveOrCancelTweet(int approveById, int cancelById)
        {
            var user = User.GetUsername();

            var tweetIdCheck = 0;
            var operationType = "";

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

            var tweetQueue = _databaseContext.TweetQueues.Find(tweetIdCheck);
            if (tweetQueue.HandleUser != user)
            {
                return BadRequest();
            }

            if (operationType == "approve")
            {
                tweetQueue.IsApprovedByHandle = true;

                if (tweetQueue.TweetUser != "AutoRetweetService")
                {
                    NotificationService.SendApprovalNotif(tweetQueue);
                }
            }
            else
            {
                tweetQueue.IsApprovedByHandle = false;
                if (tweetQueue.TweetUser != "AutoRetweetService")
                {
                    NotificationService.SendCancelNotif(tweetQueue);
                }
            }

            _databaseContext.SaveChanges();
            return Ok();
        }

        [HttpDelete, Route("api/delete-tweet")]
        public IHttpActionResult DeleteTweet(int id)
        {
            var user = User.GetUsername();

            // find tweet by id, make sure claims user == tweet user
            var tweetQueue = _databaseContext.TweetQueues.Find(id);
            if (tweetQueue.TweetUser == user)
            {
                _databaseContext.TweetQueues.Remove(tweetQueue);
                _databaseContext.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost, Route("api/post-new-tweet")]
        public IHttpActionResult PostNewTweet(TweetQueue tweetQueue)
        {
            var user = User.GetUsername();
            tweetQueue.TweetUser = user;

            // find twitter account user
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(a => a.TwitterHandle == tweetQueue.TwitterHandle);
            tweetQueue.HandleUser = account.HandleUser;

            // populate last few object fields
            tweetQueue.CreatedTime = DateTime.UtcNow;
            tweetQueue.IsApprovedByHandle = false;
            tweetQueue.IsPostedByWebJob = false;

            _databaseContext.TweetQueues.Add(tweetQueue);
            _databaseContext.SaveChanges();

            NotificationService.SendNotificationToHandle(tweetQueue);
            tweetQueue.HandleUser = null;
            return Ok(tweetQueue);
        }
    }
}