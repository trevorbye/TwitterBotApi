using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    public class AdminManagersController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();

        [Route("api/init-loader")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult InitDb()
        {
            db.TweetQueues.Find(0);
            return Ok();
        }
    }
}