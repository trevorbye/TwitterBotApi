using System;
using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    public class AdminManagersController : ApiController
    {
        private readonly TwitterBotContext _context = new TwitterBotContext();
        static readonly string AppMessageBeforeLogin = Environment.GetEnvironmentVariable("APP_MESSAGE_BEFORE_LOGIN");

        [HttpGet, Route("api/init-loader")]
        public IHttpActionResult InitDb()
        {
            _context.TweetQueues.Find(0);

            return Ok();
        }

        [HttpGet, Route("api/app-message-before-login")]
        public IHttpActionResult MessageBeforeLogin()
        {
            return Ok(AppMessageBeforeLogin);
        }
    }
}