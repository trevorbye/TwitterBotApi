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

        [Required]
        [StringLength(255)]
        public string TwitterHandle { get; set; }

        [Required]
        [StringLength(255)]
        public string TweetUser { get; set; }

        [Required]
        [StringLength(40)]
        public string HandleUser { get; set; }

        public int? ChangedThresholdPercentage { get; set; }

        public bool CodeChanges { get; set; }

        public bool External { get; set; }

        public bool NewFiles { get; set; }

        public bool IgnoreMetadataOnly { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Channel { get; set; }

        [StringLength(255)]
        public string SearchType { get; set; }

        [StringLength(255)]
        public string SearchBy { get; set; }

        [StringLength(255)]
        public string ForceNotifyTag { get; set; }

        [StringLength(1000)]
        public string QueryString { get; set; }

        [StringLength(255)]
        public string Rss { get; set; }

        [StringLength(1000)]
        public string TemplateText { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
