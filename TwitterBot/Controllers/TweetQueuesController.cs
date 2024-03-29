﻿using System;
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
    public class TweetQueuesController : ApiController
    {
        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        [HttpGet, Route("api/get-user-tweet-queue")]
        public async Task<IHttpActionResult> GetUserTweetQueue()
        {
            var user = User.GetUsername();
            IList<TweetQueue> tweets = _databaseContext.TweetQueues.Where(table => table.TweetUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();

            // get images from blob storage
            BlockBlobManager manager = new BlockBlobManager();
            var asyncTasks = new List<Task<List<string>>>();
            var idxReplaceList = new List<int>();
            int idx = 0;
            foreach (var tweet in tweets)
            {
                if (tweet.BlockBlobIdsConcat != null)
                {
                    idxReplaceList.Add(idx);
                    asyncTasks.Add(manager.GetTweetImagesAsync(tweet));
                }
                idx += 1;
            }
            var taskResult = await Task.WhenAll(asyncTasks);
            var listOfImageLists = new Queue<List<string>>(taskResult);
            foreach (var i in idxReplaceList)
            {
                tweets[i].ImageBase64Strings = listOfImageLists.Dequeue();
            }
            
            return Ok(tweets);
        }

        [HttpGet, Route("api/get-handles-tweet-queue")]
        public async Task<IHttpActionResult> GetHandlesTweetQueue()
        {
            var user = User.GetUsername();
            IList<TweetQueue> tweets = _databaseContext.TweetQueues.Where(table => table.HandleUser == user)
                .OrderByDescending(x => x.CreatedTime).ToList();

            // get images from blob storage
            BlockBlobManager manager = new BlockBlobManager();
            var asyncTasks = new List<Task<List<string>>>();
            var idxReplaceList = new List<int>();
            int idx = 0;
            foreach (var tweet in tweets)
            {
                if (tweet.BlockBlobIdsConcat != null)
                {
                    idxReplaceList.Add(idx);
                    asyncTasks.Add(manager.GetTweetImagesAsync(tweet));
                }
                idx += 1;
            }
            var taskResult = await Task.WhenAll(asyncTasks);
            var listOfImageLists = new Queue<List<string>>(taskResult);
            foreach (var i in idxReplaceList)
            {
                tweets[i].ImageBase64Strings = listOfImageLists.Dequeue();
            }

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

        [HttpGet, Route("api/get-public-schedule")]
        public IHttpActionResult GetPublicTweetSchedule(string handle)
        {
            var account = _databaseContext.TwitterAccounts.FirstOrDefault(table => table.TwitterHandle == handle);
            if (!account.IsTweetSchedulePublic)
            {
                var emptyQueue = new List<TweetQueue>();
                return Ok(emptyQueue);
            }
            else
            {
                return Ok(
                _databaseContext.TweetQueues
                    .Where(table => table.TwitterHandle == handle)
                    .Select(tweet => new
                    {
                        tweet.TwitterHandle,
                        tweet.ScheduledStatusTime,
                        tweet.IsApprovedByHandle,
                        tweet.IsPostedByWebJob,
                        tweet.Id
                    })
                    
                    .ToList()
                );
            }
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

            // if these aren't equal, the editor deleted at least one image
            if (model.BlockBlobIdsConcat != null)
            {
                if (model.ImageBase64Strings.Count != tweetQueue.GetBlockBlobIdsAsList().Count)
                {
                    BlockBlobManager manager = new BlockBlobManager();
                    manager.DeleteBlobsFromIds(tweetQueue.GetBlockBlobIdsAsList());

                    if (model.ImageBase64Strings.Count > 0)
                    {
                        List<string> blobIds = null;
                        try
                        {
                            blobIds = manager.UploadFileStreams(model.ImageBase64Strings);
                        }
                        catch (Exception e)
                        {
                            return BadRequest("Error uploading image(s) to storage.");
                        }
                        tweetQueue.SetBlockBlobIdsConcat(blobIds);
                    }
                    else
                    {
                        tweetQueue.BlockBlobIdsConcat = null;
                    }
                }
            }

            tweetQueue.StatusBody = model.StatusBody;
            _databaseContext.SaveChanges();
            NotificationService.SendEditNotif(tweetQueue, originalStatus);
        
            return Ok();
        }

        [HttpPost, Route("api/edit-tweet-attributes")]
        public IHttpActionResult ReactAppEditTweetByHandleOwner(TweetQueue tweet)
        {
            var user = User.GetUsername();

            var tweetQueue = _databaseContext.TweetQueues.Find(tweet.Id);
            var originalStatus = tweetQueue.StatusBody;
            if (tweetQueue.HandleUser != user)
            {
                return BadRequest();
            }

            tweetQueue.StatusBody = tweet.StatusBody;
            tweetQueue.ScheduledStatusTime = tweet.ScheduledStatusTime;
            _databaseContext.SaveChanges();
            NotificationService.SendEditNotif(tweetQueue, originalStatus);

            return Ok();
        }

        [HttpDelete, Route("api/delete-tweet-image")]
        public IHttpActionResult ReactAppDeleteImage(int tweetId, int imageIdx)
        {
            var user = User.GetUsername();
            var tweetQueue = _databaseContext.TweetQueues.Find(tweetId);
            if (tweetQueue.HandleUser != user)
            {
                return BadRequest();
            }
            var blobIds = tweetQueue.GetBlockBlobIdsAsList();
            BlockBlobManager manager = new BlockBlobManager();
            manager.DeleteBlobFromId(blobIds[imageIdx]);

            blobIds.RemoveAt(imageIdx);
            tweetQueue.SetBlockBlobIdsConcat(blobIds);
            _databaseContext.SaveChanges();
            
            return Ok(tweetQueue.BlockBlobIdsConcat);
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

            // find tweet by id, make sure claims user == tweet user OR handle user
            var tweetQueue = _databaseContext.TweetQueues.Find(id);
            if (tweetQueue.TweetUser == user || tweetQueue.HandleUser == user)
            {
                if (tweetQueue.BlockBlobIdsConcat != null)
                {
                    BlockBlobManager manager = new BlockBlobManager();
                    manager.DeleteBlobsFromIds(tweetQueue.GetBlockBlobIdsAsList());
                }
                
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

            // determine if tweet has attached images, if so, run validation, and upload to blob
            if (tweetQueue.ImageBase64Strings.Count > 0)
            {
                BlockBlobManager blobManager = new BlockBlobManager();
                // validate image, return bad request if not supported format
                var errors = blobManager.ValidationErrors(tweetQueue.ImageBase64Strings);
                if (errors.Count > 0)
                {
                    var message = "";
                    foreach (var error in errors)
                    {
                        message += (error + " ");
                    }
                    return Content(System.Net.HttpStatusCode.BadRequest, message);
                }

                List<string> blobIds = null;
                try
                {
                    blobIds = blobManager.UploadFileStreams(tweetQueue.ImageBase64Strings);
                }
                catch (Exception e)
                {
                    return BadRequest("Error uploading image(s) to storage.");
                }
                tweetQueue.SetBlockBlobIdsConcat(blobIds);
            }

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