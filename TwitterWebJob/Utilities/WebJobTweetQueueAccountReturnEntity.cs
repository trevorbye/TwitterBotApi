using System.Collections.Generic;

namespace TwitterWebJob
{
    public class WebJobTweetQueueAccountReturnEntity
    {
        public IList<WebJobTweetQueue> Tweets { get; set; }

        public IList<WebJobTwitterAccount> Accounts { get; set; }
    }
}