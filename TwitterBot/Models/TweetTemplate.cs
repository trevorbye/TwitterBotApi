namespace TwitterBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TweetTemplate
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string TwitterHandle { get; set; }

        [StringLength(255)]
        public string TweetUser { get; set; }

        [Required]
        [StringLength(40)]
        public string HandleUser { get; set; }

        public int? ChangedThresholdPercentage { get; set; }

        public int? CodeChanges { get; set; }

        public int? External { get; set; }

        public int? NewFiles { get; set; }

        public int? IgnoreMetadataOnly { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Channel { get; set; }

        [StringLength(255)]
        public string MsServer { get; set; }

        [StringLength(255)]
        public string GlobPath { get; set; }

        [StringLength(255)]
        public string ForceNotifyTag { get; set; }

        [StringLength(1000)]
        public string QueryString { get; set; }

        [StringLength(255)]
        public string Rss { get; set; }

        [StringLength(1000)]
        public string Attachments { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
