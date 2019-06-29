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
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TwitterBot.Models.TwitterBotContext context)
        {
            context.AdminManagers.AddOrUpdate(x => x.Id,
                new AdminManager() { Id = 1, User = "trevor.bye@microsoft.com" }
                );
        }
    }
}
