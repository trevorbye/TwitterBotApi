using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitterBot.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterBot.Models;
using System.Security.Claims;
using Moq;
using System.Security.Principal;
using System.Web;

namespace TwitterBot.Controllers.Tests
{
    [TestClass()]
    public class TweetQueuesControllerTests
    {

        [TestMethod()]
        public void WebJobUpdateTweetIdAfterPostTest()
        {
            TweetQueue tweet = new TweetQueue();
            tweet.Id = 884;
            tweet.TweetId = "";
            tweet.TweetUser = "diberry@microsoft.com";

            var context = new Mock<HttpContextBase>();
            var mockIdentity = new Mock<IIdentity>();
            context.SetupGet(x => x.User.Identity).Returns(mockIdentity.Object);
            mockIdentity.Setup(x => x.Name).Returns(tweet.TweetUser);

            var controller = new TweetQueuesController();

            // create tweet
            var insertResult = controller.PostNewTweet(tweet);

            // send tweet to twitter


            // update db with tweet id
            var updateResult = controller.WebJobUpdateTweetIdAfterPost(tweet);

            Assert.Fail();
        }
    }
}