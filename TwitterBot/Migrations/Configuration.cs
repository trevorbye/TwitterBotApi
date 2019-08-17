namespace TwitterBot.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TwitterBot.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TwitterBot.Models.TwitterBotContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(TwitterBot.Models.TwitterBotContext context)
        {
            
        }
    }
}
