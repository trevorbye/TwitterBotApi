using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    public class AdminManagersController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();

        [Route("api/init-loader")]
        [HttpGet]
        public IHttpActionResult InitDb()
        {
            db.TweetQueues.Find(0);
            return Ok();
        }
    }
}