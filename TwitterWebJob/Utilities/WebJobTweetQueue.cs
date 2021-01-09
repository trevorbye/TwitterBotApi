using System;
using System.Linq;
using System.Collections.Generic;

namespace TwitterWebJob
{
    public class WebJobTweetQueue
    {
        public int Id { get; set; }

        public string TweetId { get; set; }

        public string TwitterHandle { get; set; }

        public string TweetUser { get; set; }

        public string HandleUser { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ScheduledStatusTime { get; set; }

        public string StatusBody { get; set; }

        public long RetweetNum { get; set; }

        public string BlockBlobIdsConcat { get; set; }

        public bool IsApprovedByHandle { get; set; }

        public bool IsPostedByWebJob { get; set; }
    }
}