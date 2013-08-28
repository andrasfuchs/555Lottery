namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130827a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "PoolRatios", c => c.String(nullable: false));
            AddColumn("dbo.Draws", "ValidGameCount", c => c.Int());
            AddColumn("dbo.Draws", "Pool0p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool0p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool1p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool1p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool2p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool2p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool3p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool3p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool4p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool4p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool5p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool5p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceNextDrawBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceMonthEndBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceYearEndBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.TicketLots", "SecondChanceWinner", c => c.Boolean());
            AddColumn("dbo.Games", "Hits", c => c.String());
            AlterColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(precision: 24, scale: 8));
            AlterColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(precision: 24, scale: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AlterColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            DropColumn("dbo.Games", "Hits");
            DropColumn("dbo.TicketLots", "SecondChanceWinner");
            DropColumn("dbo.Draws", "PoolSecondChanceYearEndBTC");
            DropColumn("dbo.Draws", "PoolSecondChanceMonthEndBTC");
            DropColumn("dbo.Draws", "PoolSecondChanceNextDrawBTC");
            DropColumn("dbo.Draws", "Pool5p1BTC");
            DropColumn("dbo.Draws", "Pool5p0BTC");
            DropColumn("dbo.Draws", "Pool4p1BTC");
            DropColumn("dbo.Draws", "Pool4p0BTC");
            DropColumn("dbo.Draws", "Pool3p1BTC");
            DropColumn("dbo.Draws", "Pool3p0BTC");
            DropColumn("dbo.Draws", "Pool2p1BTC");
            DropColumn("dbo.Draws", "Pool2p0BTC");
            DropColumn("dbo.Draws", "Pool1p1BTC");
            DropColumn("dbo.Draws", "Pool1p0BTC");
            DropColumn("dbo.Draws", "Pool0p1BTC");
            DropColumn("dbo.Draws", "Pool0p0BTC");
            DropColumn("dbo.Draws", "ValidGameCount");
            DropColumn("dbo.Draws", "PoolRatios");
        }
    }
}
