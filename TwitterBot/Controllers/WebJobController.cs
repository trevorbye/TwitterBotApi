using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;
using TwitterBot.POCOS;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Security.Claims;

namespace TwitterBot.Controllers
{
    public class WebJobController : ApiController
    {
        private TwitterBotContext db = new TwitterBotContext();
        private string _bearerToken = Environment.GetEnvironmentVariable("WEBJOB_AUTH_KEY");

        [Route("api/webjob-fetch-queue")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult FetchTweetQueueAndAccounts()
        {
            string authToken = Request.Headers.Authorization.Parameter;
            if (authToken != _bearerToken || authToken == null)
            {
                return Unauthorized();
            }

            return Ok();
        }
    }
}
