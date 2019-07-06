using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        [Column(TypeName = "varchar")]
        [MaxLength(280)]
        public string StatusBody { get; set; }

        public bool IsApprovedByHandle { get; set; }
        public bool IsCanceledByHandle { get; set; }
    }
}