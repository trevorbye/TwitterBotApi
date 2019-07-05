namespace TwitterBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TweetQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TwitterHandle = c.String(),
                        TweetUser = c.String(),
                        HandleUser = c.String(),
                        CreatedTime = c.DateTime(nullable: false),
                        ScheduledStatusTime = c.DateTime(nullable: false),
                        StatusBody = c.String(maxLength: 280),
                        IsApprovedByHandle = c.Boolean(nullable: false),
                        IsCanceledByHandle = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TweetQueues");
        }
    }
}
