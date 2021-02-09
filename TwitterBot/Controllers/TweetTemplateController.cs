using System;
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
    public class TweetTemplateController : ApiController
    {

        readonly TwitterBotContext _databaseContext = new TwitterBotContext();

        [HttpGet, Route("api/tweet-templates")]
        public IHttpActionResult GetAllTemplates()
        {
            var list = _databaseContext.TweetTemplates.ToList();
            return Ok(list);
        }
    }
}
