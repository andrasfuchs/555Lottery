namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Draws",
                c => new
                    {
                        DrawId = c.Int(nullable: false, identity: true),
                        DeadlineUtc = c.DateTime(nullable: false),
                        DrawCode = c.String(nullable: false),
                        JackpotBTC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BitCoinAddress = c.String(nullable: false),
                        WinningTicketSequence = c.String(),
                    })
                .PrimaryKey(t => t.DrawId);
            
            CreateTable(
                "dbo.TicketSets",
                c => new
                    {
                        TicketSetId = c.Int(nullable: false, identity: true),
                        CreatedUtc = c.DateTime(nullable: false),
                        TotalBTC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RefundAddress = c.String(nullable: false),
                        Owner_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.TicketSetId)
                .ForeignKey("dbo.Users", t => t.Owner_UserId)
                .Index(t => t.Owner_UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 50),
                        NotificationFlags = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        CreatedUtc = c.DateTime(nullable: false),
                        Mode = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Sequence = c.String(nullable: false),
                        Winnings = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.TicketId);
            
            CreateTable(
                "dbo.ExchangeRates",
                c => new
                    {
                        ExchangeRateId = c.Int(nullable: false, identity: true),
                        CurrencyISO1 = c.String(nullable: false),
                        CurrencyISO2 = c.String(nullable: false),
                        TimeUtc = c.DateTime(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ExchangeRateId);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogId = c.Int(nullable: false, identity: true),
                        UtcTime = c.DateTime(nullable: false),
                        IPAddress = c.String(nullable: false),
                        SessionId = c.String(nullable: false),
                        Action = c.String(nullable: false),
                        Parameters = c.String(),
                        FormattedMessage = c.String(),
                    })
                .PrimaryKey(t => t.LogId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.TicketSets", new[] { "Owner_UserId" });
            DropForeignKey("dbo.TicketSets", "Owner_UserId", "dbo.Users");
            DropTable("dbo.Logs");
            DropTable("dbo.ExchangeRates");
            DropTable("dbo.Tickets");
            DropTable("dbo.Users");
            DropTable("dbo.TicketSets");
            DropTable("dbo.Draws");
        }
    }
}
