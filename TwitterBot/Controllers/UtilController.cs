using System;
using System.Web.Http;

namespace TwitterBot.Controllers
{
    public class UtilController : ApiController
    {
        [HttpGet, Route("api/get-utc-now")]
        public IHttpActionResult GetUserTweetQueue() => Ok(DateTime.UtcNow);
    }
}