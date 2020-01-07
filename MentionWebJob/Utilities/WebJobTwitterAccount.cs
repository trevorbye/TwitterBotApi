namespace MentionWebJob
{
    public class WebJobTwitterAccount
    {
        public int Id { get; set; }

        public string TwitterHandle { get; set; }

        public long TwitterUserId { get; set; }

        public string HandleUser { get; set; }

        public string OauthToken { get; set; }

        public string OauthSecret { get; set; }

        public bool IsAutoRetweetEnabled { get; set; }

        public long RetweetSinceId { get; set; }
    }
}