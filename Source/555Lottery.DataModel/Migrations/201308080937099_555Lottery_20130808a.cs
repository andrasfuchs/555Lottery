namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130808a : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransactionLogs",
                c => new
                    {
                        TransactionLogId = c.Int(nullable: false, identity: true),
                        DownloadedUtc = c.DateTime(nullable: false),
                        InputAddress = c.String(nullable: false),
                        OutputAddress = c.String(nullable: false),
                        TransactionHash = c.String(nullable: false),
                        BlockHeight = c.Int(nullable: false),
                        BlockTimeStampUtc = c.DateTime(nullable: false),
                        TotalInput = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalOutput = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Confirmations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionLogId);
            
            AddColumn("dbo.TicketLots", "MostRecentTransactionLog_TransactionLogId", c => c.Int());
            AddForeignKey("dbo.TicketLots", "MostRecentTransactionLog_TransactionLogId", "dbo.TransactionLogs", "TransactionLogId");
            CreateIndex("dbo.TicketLots", "MostRecentTransactionLog_TransactionLogId");
            DropTable("dbo.TransactionHistories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TransactionHistories",
                c => new
                    {
                        TransactionHistoryId = c.Int(nullable: false, identity: true),
                        DownloadedUtc = c.DateTime(nullable: false),
                        InputAddress = c.String(nullable: false),
                        OutputAddress = c.String(nullable: false),
                        TransactionHash = c.String(nullable: false),
                        BlockHeight = c.Int(nullable: false),
                        BlockTimeStampUtc = c.DateTime(nullable: false),
                        TotalInput = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalOutput = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Confirmations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionHistoryId);
            
            DropIndex("dbo.TicketLots", new[] { "MostRecentTransactionLog_TransactionLogId" });
            DropForeignKey("dbo.TicketLots", "MostRecentTransactionLog_TransactionLogId", "dbo.TransactionLogs");
            DropColumn("dbo.TicketLots", "MostRecentTransactionLog_TransactionLogId");
            DropTable("dbo.TransactionLogs");
        }
    }
}
