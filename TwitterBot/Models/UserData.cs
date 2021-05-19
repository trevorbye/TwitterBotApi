using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class UserData
    {
        public List<TweetQueue> Tweets;
        public List<TwitterAccount> Accounts;
        public List<TweetTemplate> Templates;
    }
}