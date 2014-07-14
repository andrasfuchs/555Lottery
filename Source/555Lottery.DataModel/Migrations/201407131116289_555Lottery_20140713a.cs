namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20140713a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "ExpectedWinningsBTC", c => c.String(nullable: false));
            DropColumn("dbo.Draws", "PoolRatios");
            DropColumn("dbo.Draws", "AmountInPools");
            DropColumn("dbo.Draws", "PoolSecondChanceNextDrawBTC");
            DropColumn("dbo.Draws", "PoolSecondChanceMonthEndBTC");
            DropColumn("dbo.Draws", "PoolSecondChanceYearEndBTC");
            DropColumn("dbo.Draws", "SecondChanceWinningsBTC");
            DropColumn("dbo.TicketLots", "SecondChanceParticipant");
            DropColumn("dbo.TicketLots", "SecondChanceWinner");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TicketLots", "SecondChanceWinner", c => c.Boolean());
            AddColumn("dbo.TicketLots", "SecondChanceParticipant", c => c.Boolean());
            AddColumn("dbo.Draws", "SecondChanceWinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceYearEndBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceMonthEndBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "PoolSecondChanceNextDrawBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "AmountInPools", c => c.String());
            AddColumn("dbo.Draws", "PoolRatios", c => c.String(nullable: false));
            DropColumn("dbo.Draws", "ExpectedWinningsBTC");
        }
    }
}
