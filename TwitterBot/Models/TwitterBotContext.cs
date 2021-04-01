using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TwitterBot.Models
{
    public class TwitterBotContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public TwitterBotContext() : base("name=TwitterBotContext")
        {
            // Remove Database Checks 
            // If DB is out of sync from latest migration - don't throw 500s on API endpoint calls
            // Issue: https://github.com/dotnet/ef6/issues/1824#event-4404180303
            // Docs: https://www.entityframeworktutorial.net/code-first/database-initialization-strategy-in-code-first.aspx
            Database.SetInitializer <TwitterBotContext>(new NullDatabaseInitializer<TwitterBotContext>());
        }

        public DbSet<AdminManager> AdminManagers { get; set; }

        public DbSet<TweetQueue> TweetQueues { get; set; }

        public DbSet<TwitterAccount> TwitterAccounts { get; set; }

        public DbSet<TweetTemplate> TweetTemplates { get; set; }

    }
}
