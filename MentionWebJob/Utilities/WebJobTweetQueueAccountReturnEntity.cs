using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentionWebJob
{
    class WebJobTweetQueueAccountReturnEntity
    {
        public IList<WebJobTweetQueue> Tweets { get; set; }
        public IDictionary<string, WebJobTwitterAccount> Accounts { get; set; }
    }
}
