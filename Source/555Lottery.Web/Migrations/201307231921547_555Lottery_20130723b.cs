namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130723b : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TicketLots", "RefundAddress", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TicketLots", "RefundAddress", c => c.String(nullable: false));
        }
    }
}
