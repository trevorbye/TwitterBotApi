using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterBot.Models
{
    public class TweetQueue
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(40)]
        public string TwitterHandle { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(40)]
        public string TweetUser { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(40)]
        public string HandleUser { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime ScheduledStatusTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1000)]
        public string StatusBody { get; set; }

        // this field is used to signal that this is a @mention tweet, and also holds the tweetId that a service would use to retweet this particular tweet.
        public long RetweetNum { get; set; }

        public bool IsApprovedByHandle { get; set; }
        public bool IsPostedByWebJob { get; set; }

    }
}