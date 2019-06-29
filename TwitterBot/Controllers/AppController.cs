using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TwitterBot.Models;

namespace TwitterBot.Controllers
{
    [RoutePrefix("v1/service-tests")]
    public class AppController : ApiController
    {
        [Route("service-status")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult FetchStatus()
        {
            RestTest restTest = new RestTest("running");
            return Ok(restTest);
        }
    }
}
