using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    public class AdminManagersController : ApiController
    {
        private readonly TwitterBotContext _context = new TwitterBotContext();

        [HttpGet, Route("api/init-loader")]
        public IHttpActionResult InitDb()
        {
            _context.TweetQueues.Find(0);

            return Ok();
        }
    }
}