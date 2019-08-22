using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class TweetQueueAccountReturnEntity
    {
        public IList<TweetQueue> Tweets { get; set; }
        public IDictionary<string, TwitterAccount> Accounts { get; set; }

        public TweetQueueAccountReturnEntity(IList<TweetQueue> tweets, IDictionary<string, TwitterAccount> accounts)
        {
            Tweets = tweets;
            Accounts = accounts;
        }
    }
}