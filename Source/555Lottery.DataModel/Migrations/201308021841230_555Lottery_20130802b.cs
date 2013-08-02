namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130802b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketLots", "Code", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketLots", "Code");
        }
    }
}
