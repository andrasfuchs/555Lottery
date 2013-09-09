namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130909a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "TotalIncomeBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "AmountInPools", c => c.String());
            AddColumn("dbo.Draws", "WinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "SecondChanceWinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Hits", c => c.String());
            AddColumn("dbo.TicketLots", "WinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.TicketLots", "MostRecentPayoutTransactionLog_TransactionLogId", c => c.Int());
            AddColumn("dbo.Tickets", "WinningsBTC", c => c.Decimal(precision: 24, scale: 8));
            AddForeignKey("dbo.TicketLots", "MostRecentPayoutTransactionLog_TransactionLogId", "dbo.TransactionLogs", "TransactionLogId");
            CreateIndex("dbo.TicketLots", "MostRecentPayoutTransactionLog_TransactionLogId");
            DropColumn("dbo.Draws", "Pool0p0BTC");
            DropColumn("dbo.Draws", "Pool0p1BTC");
            DropColumn("dbo.Draws", "Pool1p0BTC");
            DropColumn("dbo.Draws", "Pool1p1BTC");
            DropColumn("dbo.Draws", "Pool2p0BTC");
            DropColumn("dbo.Draws", "Pool2p1BTC");
            DropColumn("dbo.Draws", "Pool3p0BTC");
            DropColumn("dbo.Draws", "Pool3p1BTC");
            DropColumn("dbo.Draws", "Pool4p0BTC");
            DropColumn("dbo.Draws", "Pool4p1BTC");
            DropColumn("dbo.Draws", "Pool5p0BTC");
            DropColumn("dbo.Draws", "Pool5p1BTC");
            DropColumn("dbo.Draws", "NonJackpotWonBTC");
            DropColumn("dbo.Draws", "SecondChanceWonBTC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool5p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool5p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool4p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool4p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool3p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool3p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool2p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool2p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool1p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool1p0BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool0p1BTC", c => c.Decimal(precision: 24, scale: 8));
            AddColumn("dbo.Draws", "Pool0p0BTC", c => c.Decimal(precision: 24, scale: 8));
            DropIndex("dbo.TicketLots", new[] { "MostRecentPayoutTransactionLog_TransactionLogId" });
            DropForeignKey("dbo.TicketLots", "MostRecentPayoutTransactionLog_TransactionLogId", "dbo.TransactionLogs");
            DropColumn("dbo.Tickets", "WinningsBTC");
            DropColumn("dbo.TicketLots", "MostRecentPayoutTransactionLog_TransactionLogId");
            DropColumn("dbo.TicketLots", "WinningsBTC");
            DropColumn("dbo.Draws", "Hits");
            DropColumn("dbo.Draws", "SecondChanceWinningsBTC");
            DropColumn("dbo.Draws", "WinningsBTC");
            DropColumn("dbo.Draws", "AmountInPools");
            DropColumn("dbo.Draws", "TotalIncomeBTC");
        }
    }
}
