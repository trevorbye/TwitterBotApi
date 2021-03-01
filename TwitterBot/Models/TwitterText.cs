using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class TwitterTextValidationResponse
    {
        public string textReturn { get; set; }
        public bool isValid { get; set; }
    }

    public class TwitterTextValidationRequest
    {
        public string tweetText;
    }
}