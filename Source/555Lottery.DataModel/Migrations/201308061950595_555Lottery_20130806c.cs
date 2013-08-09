namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130806c : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionHistories", "PrimaryFromAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "PrimaryToAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "ReceivedAmountToPrimary", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TransactionHistories", "FromAddress");
            DropColumn("dbo.TransactionHistories", "ToAddress");
            DropColumn("dbo.TransactionHistories", "Amount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionHistories", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TransactionHistories", "ToAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "FromAddress", c => c.String(nullable: false));
            DropColumn("dbo.TransactionHistories", "ReceivedAmountToPrimary");
            DropColumn("dbo.TransactionHistories", "PrimaryToAddress");
            DropColumn("dbo.TransactionHistories", "PrimaryFromAddress");
        }
    }
}
