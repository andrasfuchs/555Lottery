namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130809d : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Draws", "JackpotBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.TicketLots", "TotalBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.TicketLots", "TotalBTCDiscount", c => c.Decimal(nullable: false, precision: 24, scale: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TicketLots", "TotalBTCDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.TicketLots", "TotalBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Draws", "JackpotBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
