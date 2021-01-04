using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterBot.Models
{
    public class TweetQueue
    {
        public int Id { get; set; }

        public string TweetId { get; set; }

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

        // this field is used to store the blobIDs for images uploaded with the tweet. ':' is the delimiter
        // yes, I know it's heresy to do this in SQL; we're doing it anyway
        [Column(TypeName = "varchar")]
        [MaxLength(150)]
        public string BlockBlobIdsConcat { get; set; }

        // this field is used to store the poll content. ':' is the delimiter
        [Column(TypeName = "varchar")]
        [MaxLength(150)]
        public string Poll { get; set; }
        public string PollId { get; set; }
        public int PollDurationMinutes { get; set; }
        

        public bool IsApprovedByHandle { get; set; }
        public bool IsPostedByWebJob { get; set; }

        [NotMapped]
        // this field primarily used as convenience to deserialize json from front end that contains image base64 strings
        public List<string> ImageBase64Strings { get; set; }

        public List<string> GetBlockBlobIdsAsList()
        {
            return this.BlockBlobIdsConcat.Split(':').ToList();
        }

        public void SetBlockBlobIdsConcat(List<string> blobIds)
        {
            if (blobIds.Count == 0)
            {
                BlockBlobIdsConcat = null;
                return;
            }

            string concatIds = "";
            foreach (var s in blobIds)
            {
                concatIds += s;
                concatIds += ':';
            }
            BlockBlobIdsConcat = concatIds.TrimEnd(':');
        }
    }
}