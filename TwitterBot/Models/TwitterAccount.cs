using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterBot.Models
{
    public class TwitterAccount
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(40)]
        public string TwitterHandle { get; set; }

        public long TwitterUserId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(40)]
        public string HandleUser { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string OauthToken { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string OauthSecret { get; set; }

        public bool IsAutoRetweetEnabled { get; set; }

        // this field keeps track of the last-fetched tweet in the mention timeline. This is used in the api call that grabs the mentions timeline, to avoid adding retweets to someones queue that have 
        // already been added previously
        public long RetweetSinceId { get; set; }

        public bool IsPrivateAccount { get; set; }

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