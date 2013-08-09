namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130809c : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Draws", "WinningTicketGeneratedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Draws", "WinningTicketGeneratedAt", c => c.DateTime(nullable: false));
        }
    }
}
