namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130905b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "Level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Logs", "Level");
        }
    }
}
