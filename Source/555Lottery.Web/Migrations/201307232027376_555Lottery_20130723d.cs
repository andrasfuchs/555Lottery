namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130723d : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tickets", "Sequence", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tickets", "Sequence");
        }
    }
}
