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
        [MaxLength(280)]
        public string StatusBody { get; set; }

        public bool IsApprovedByHandle { get; set; }
        public bool IsPostedByWebJob { get; set; }

    }
}