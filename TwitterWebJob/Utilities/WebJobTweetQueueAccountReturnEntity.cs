using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterWebJob
{
    public class WebJobTweetQueueAccountReturnEntity
    {
        public IList<WebJobTweetQueue> Tweets { get; set; }
        public IDictionary<string, WebJobTwitterAccount> Accounts { get; set; }

        public WebJobTweetQueueAccountReturnEntity(IList<WebJobTweetQueue> tweets, IDictionary<string, WebJobTwitterAccount> accounts)
        {
            Tweets = tweets;
            Accounts = accounts;
        }
    }
}