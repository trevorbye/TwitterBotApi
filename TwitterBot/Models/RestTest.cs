using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class RestTest
    {
        public string ServiceStatus { get; set; }

        public RestTest (string status)
        {
            ServiceStatus = status;
        }
    }
}