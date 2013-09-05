namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130905a : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TicketLots", "SessionId");
            DropColumn("dbo.TicketLots", "EmailAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TicketLots", "EmailAddress", c => c.String());
            AddColumn("dbo.TicketLots", "SessionId", c => c.String());
        }
    }
}
