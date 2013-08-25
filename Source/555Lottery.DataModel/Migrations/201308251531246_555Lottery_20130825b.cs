namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130825b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketLots", "TotalDiscountBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Draws", "OneGameBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Games", "WinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            DropColumn("dbo.TicketLots", "TotalBTCDiscount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TicketLots", "TotalBTCDiscount", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Games", "WinningsBTC", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Draws", "OneGameBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TicketLots", "TotalDiscountBTC");
        }
    }
}
