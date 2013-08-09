namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130806d : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionHistories", "InputAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "OutputAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "TotalInput", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TransactionHistories", "TotalOutput", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TransactionHistories", "PrimaryFromAddress");
            DropColumn("dbo.TransactionHistories", "PrimaryToAddress");
            DropColumn("dbo.TransactionHistories", "ReceivedAmountToPrimary");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionHistories", "ReceivedAmountToPrimary", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TransactionHistories", "PrimaryToAddress", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "PrimaryFromAddress", c => c.String(nullable: false));
            DropColumn("dbo.TransactionHistories", "TotalOutput");
            DropColumn("dbo.TransactionHistories", "TotalInput");
            DropColumn("dbo.TransactionHistories", "OutputAddress");
            DropColumn("dbo.TransactionHistories", "InputAddress");
        }
    }
}
