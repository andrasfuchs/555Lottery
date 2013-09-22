namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130922a : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ExchangeRates", "CurrencyISO1", c => c.String(nullable: false, maxLength: 3, fixedLength: true, unicode: false));
            AlterColumn("dbo.ExchangeRates", "CurrencyISO2", c => c.String(nullable: false, maxLength: 3, fixedLength: true, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ExchangeRates", "CurrencyISO2", c => c.String(nullable: false));
            AlterColumn("dbo.ExchangeRates", "CurrencyISO1", c => c.String(nullable: false));
        }
    }
}
