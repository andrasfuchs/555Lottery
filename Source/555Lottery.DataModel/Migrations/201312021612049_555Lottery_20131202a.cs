namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20131202a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Name", c => c.String());
            AddColumn("dbo.Users", "ReturnBitcoinAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ReturnBitcoinAddress");
            DropColumn("dbo.Users", "Name");
        }
    }
}
