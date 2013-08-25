namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130825c : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionLogs", "TotalInputBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            AddColumn("dbo.TransactionLogs", "OutputBTC", c => c.Decimal(nullable: false, precision: 24, scale: 8));
            DropColumn("dbo.TransactionLogs", "TotalInput");
            DropColumn("dbo.TransactionLogs", "TotalOutput");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionLogs", "TotalOutput", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TransactionLogs", "TotalInput", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.TransactionLogs", "OutputBTC");
            DropColumn("dbo.TransactionLogs", "TotalInputBTC");
        }
    }
}
