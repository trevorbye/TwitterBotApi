using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TwitterBot.Models
{
    public class TweetQueue
    {
        public int Id { get; set; }
        public string TwitterHandle { get; set; }
        public string TweetUser { get; set; }
        public string HandleUser { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ScheduledStatusTime { get; set; }
        [MaxLength(280)]
        public string StatusBody { get; set; }
        public bool IsApprovedByHandle { get; set; }
        public bool IsCanceledByHandle { get; set; }
    }
}