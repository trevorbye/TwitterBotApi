using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterBot.Models
{
    public class TwitterAccount
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(40)]
        public string TwitterHandle { get; set; }

        public long TwitterUserId { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(40)]
        public string HandleUser { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string OauthToken { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string OauthSecret { get; set; }

        public bool IsAutoRetweetEnabled { get; set; }

        public TwitterAccount(string handle, long twitterUserId, string handleUser, string oauthToken, string  oauthSecret, bool enableRetweets)
        {
            TwitterHandle = handle;
            TwitterUserId = twitterUserId;
            HandleUser = handleUser;
            OauthToken = oauthToken;
            OauthSecret = oauthSecret;
            IsAutoRetweetEnabled = enableRetweets;
        }

        public TwitterAccount() { }
    }
}