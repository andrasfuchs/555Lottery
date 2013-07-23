namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130723c : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tickets", "Sequence");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "Sequence", c => c.String(nullable: false));
        }
    }
}
