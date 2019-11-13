namespace TwitterBot.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TwitterBot.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TwitterBotContext>
    {
        
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
        
        protected override void Seed(TwitterBotContext context)
        {
            
        }
        
    }
}
