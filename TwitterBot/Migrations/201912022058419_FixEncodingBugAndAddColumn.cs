namespace TwitterBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixEncodingBugAndAddColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TwitterAccounts", "IsPrivateAccount", c => c.Boolean(nullable: false));
            AlterColumn("dbo.TweetQueues", "StatusBody", c => c.String(maxLength: 280));
            AlterColumn("dbo.TwitterAccounts", "TwitterHandle", c => c.String(maxLength: 40));
            AlterColumn("dbo.TwitterAccounts", "HandleUser", c => c.String(maxLength: 40));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TwitterAccounts", "HandleUser", c => c.String(maxLength: 40, unicode: false));
            AlterColumn("dbo.TwitterAccounts", "TwitterHandle", c => c.String(maxLength: 40, unicode: false));
            AlterColumn("dbo.TweetQueues", "StatusBody", c => c.String(maxLength: 280, unicode: false));
            DropColumn("dbo.TwitterAccounts", "IsPrivateAccount");
        }
    }
}
