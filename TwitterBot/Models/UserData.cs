using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class UserData
    {
        public IQueryable<TweetQueue> Tweets;
        public IQueryable<TwitterAccount> Accounts;
        public IQueryable<TweetTemplate> Templates;
    }
}