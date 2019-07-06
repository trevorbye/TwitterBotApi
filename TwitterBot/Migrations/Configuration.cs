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
            context.AdminManagers.AddOrUpdate(x => x.Id,
                new AdminManager() { Id = 1, User = "trbye@microsoft.com" }
                );

            context.TweetQueues.AddOrUpdate(x => x.Id,
                new TweetQueue()
                {
                    Id = 1,
                    TweetUser = "trbye@microsoft.com",
                    CreatedTime = DateTime.Now,
                    HandleUser = "test",
                    IsApprovedByHandle = false,
                    IsCanceledByHandle = false,
                    ScheduledStatusTime = DateTime.Now,
                    StatusBody = "this is a test tweet.",
                    TwitterHandle = "@testHandle"
                },
                new TweetQueue()
                {
                    Id = 1,
                    TweetUser = "trbye@microsoft.com",
                    CreatedTime = DateTime.Now,
                    HandleUser = "test",
                    IsApprovedByHandle = true,
                    IsCanceledByHandle = false,
                    ScheduledStatusTime = DateTime.Now,
                    StatusBody = "Testing another tweet. This one is slightly longer.",
                    TwitterHandle = "@testHandle"
                },
                new TweetQueue()
                {
                    Id = 1,
                    TweetUser = "trbye@microsoft.com",
                    CreatedTime = DateTime.Now,
                    HandleUser = "test",
                    IsApprovedByHandle = false,
                    IsCanceledByHandle = false,
                    ScheduledStatusTime = DateTime.Now,
                    StatusBody = "Visit our new article on Azure Machine Learning hyperparameter tuning.",
                    TwitterHandle = "@testHandle"
                }
                );
        }
    }
}
