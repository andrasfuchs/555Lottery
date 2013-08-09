namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130806a : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransactionHistories",
                c => new
                    {
                        TransactionHistoryId = c.Int(nullable: false, identity: true),
                        DownloadedUtc = c.DateTime(nullable: false),
                        FromAddress = c.String(nullable: false),
                        ToAddress = c.String(nullable: false),
                        TransactionId = c.String(nullable: false),
                        BlockId = c.String(nullable: false),
                        BlockTimeStampUtc = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Confirmations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionHistoryId);
            
            AddColumn("dbo.TicketLots", "TotalBTCDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TicketLots", "EmailAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketLots", "EmailAddress");
            DropColumn("dbo.TicketLots", "TotalBTCDiscount");
            DropTable("dbo.TransactionHistories");
        }
    }
}
