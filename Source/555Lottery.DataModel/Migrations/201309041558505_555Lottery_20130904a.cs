namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130904a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "SessionId", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Email", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Users", "SessionId");
        }
    }
}
