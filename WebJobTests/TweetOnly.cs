using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Text;
using System.Net;
using TwitterWebJob;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebJobTests
{
    [TestClass]
    public class TweetOnly
    {

        [TestMethod]
        public void SendTweetFromWebActionClass()
        {
            // User
            WebJobTwitterAccount dinaTest = new WebJobTwitterAccount();
            dinaTest.Id = 37;
            dinaTest.TwitterHandle = "@TDfberry";
            dinaTest.TwitterUserId = 1334988613025554434;
            dinaTest.HandleUser = "diberry@microsoft.com";
            dinaTest.OauthSecret = "PFO1EDmrogBtOU2xi6wibA6B0d9mbjJHWCI2g0Ze7Rfyl";
            dinaTest.OauthToken = "1334988613025554434-QfZ1rtqoCbb6nrAADTX7jTXIYkIp9r";
            dinaTest.IsAutoRetweetEnabled = false;
            var accountsList = new List<WebJobTwitterAccount>();
            accountsList.Add(dinaTest);

            // Tweet
            WebJobTweetQueue tweetQueue = new WebJobTweetQueue();
            tweetQueue.Id = 884;
            tweetQueue.TwitterHandle = "@TDfberry";
            tweetQueue.TweetUser = "diberry@microsoft.com";
            tweetQueue.StatusBody = "testing ... " + DateTime.UtcNow.ToString();
            tweetQueue.IsApprovedByHandle = true;
            tweetQueue.BlockBlobIdsConcat = null;
            tweetQueue.Poll = null;
            tweetQueue.hasPoll = false;

            // Auth
            var bearer = $"Bearer {dinaTest.OauthToken}";

            // set environment variables for webjob
            // doesn't work with API project, set those separately
            Environment.SetEnvironmentVariable("TWITTERBOT_API_URI", "http://localhost:52937/");
            Environment.SetEnvironmentVariable("CONSUMER_KEY", "E4BdX6Shn32xHNX4ydpW1Kj0b");
            Environment.SetEnvironmentVariable("SECRET", "g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf");
            Environment.SetEnvironmentVariable("WEBJOB_AUTH_KEY", "Bearer g76QnyQSi0khjuJcZ4TPcBbsKISHmh2YDhWqcylZ8CYRtiz7zf");

            // returns new tweet id
            string result = WebActions.SendTweetAsync(tweetQueue, accountsList, bearer).Result;

            Assert.IsTrue(result != null);
        }
    }

}
