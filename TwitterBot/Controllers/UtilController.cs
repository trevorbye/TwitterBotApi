using System;
using System.Web.Http;

namespace TwitterBot.Controllers
{
    public class UtilController : ApiController
    {
        [Route("api/get-utc-now")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserTweetQueue()
        {
            return Ok(DateTime.UtcNow);
        }
    }
}