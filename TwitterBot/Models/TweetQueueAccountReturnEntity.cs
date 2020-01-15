using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class TweetQueueAccountReturnEntity
    {
        public IList<TweetQueue> Tweets { get; set; }
        public IList<TwitterAccount> Accounts { get; set; }

        public TweetQueueAccountReturnEntity(IList<TweetQueue> tweets, IList<TwitterAccount> accounts)
        {
            Tweets = tweets;
            Accounts = accounts;
        }
    }
}