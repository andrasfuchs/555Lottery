namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130806b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionHistories", "TransactionHash", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "BlockHeight", c => c.Int(nullable: false));
            DropColumn("dbo.TransactionHistories", "TransactionId");
            DropColumn("dbo.TransactionHistories", "BlockId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionHistories", "BlockId", c => c.String(nullable: false));
            AddColumn("dbo.TransactionHistories", "TransactionId", c => c.String(nullable: false));
            DropColumn("dbo.TransactionHistories", "BlockHeight");
            DropColumn("dbo.TransactionHistories", "TransactionHash");
        }
    }
}
