namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130720b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "JackpotUSDAtDeadline", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Draws", "JackpotEURAtDeadline", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Draws", "JackpotEURAtDeadline");
            DropColumn("dbo.Draws", "JackpotUSDAtDeadline");
        }
    }
}
