namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130821a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "NonJackpotWonBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Draws", "SecondChanceWonBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Draws", "ExchangeRateUSDAtDeadline_ExchangeRateId", c => c.Int());
            AddColumn("dbo.Draws", "ExchangeRateEURAtDeadline_ExchangeRateId", c => c.Int());
            AddColumn("dbo.Tickets", "Index", c => c.Int(nullable: false));
            AddForeignKey("dbo.Draws", "ExchangeRateUSDAtDeadline_ExchangeRateId", "dbo.ExchangeRates", "ExchangeRateId");
            AddForeignKey("dbo.Draws", "ExchangeRateEURAtDeadline_ExchangeRateId", "dbo.ExchangeRates", "ExchangeRateId");
            CreateIndex("dbo.Draws", "ExchangeRateUSDAtDeadline_ExchangeRateId");
            CreateIndex("dbo.Draws", "ExchangeRateEURAtDeadline_ExchangeRateId");
            DropColumn("dbo.Draws", "JackpotUSDAtDeadline");
            DropColumn("dbo.Draws", "JackpotEURAtDeadline");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Draws", "JackpotEURAtDeadline", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Draws", "JackpotUSDAtDeadline", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropIndex("dbo.Draws", new[] { "ExchangeRateEURAtDeadline_ExchangeRateId" });
            DropIndex("dbo.Draws", new[] { "ExchangeRateUSDAtDeadline_ExchangeRateId" });
            DropForeignKey("dbo.Draws", "ExchangeRateEURAtDeadline_ExchangeRateId", "dbo.ExchangeRates");
            DropForeignKey("dbo.Draws", "ExchangeRateUSDAtDeadline_ExchangeRateId", "dbo.ExchangeRates");
            DropColumn("dbo.Tickets", "Index");
            DropColumn("dbo.Draws", "ExchangeRateEURAtDeadline_ExchangeRateId");
            DropColumn("dbo.Draws", "ExchangeRateUSDAtDeadline_ExchangeRateId");
            DropColumn("dbo.Draws", "SecondChanceWonBTC");
            DropColumn("dbo.Draws", "NonJackpotWonBTC");
        }
    }
}
