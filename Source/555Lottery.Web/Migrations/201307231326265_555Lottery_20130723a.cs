namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130723a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "OneGamePrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.TicketLots", "SessionId", c => c.String());
            AddColumn("dbo.Tickets", "TicketLot_TicketLotId", c => c.Int(nullable: false));
            AddForeignKey("dbo.Tickets", "TicketLot_TicketLotId", "dbo.TicketLots", "TicketLotId", cascadeDelete: true);
            CreateIndex("dbo.Tickets", "TicketLot_TicketLotId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tickets", new[] { "TicketLot_TicketLotId" });
            DropForeignKey("dbo.Tickets", "TicketLot_TicketLotId", "dbo.TicketLots");
            DropColumn("dbo.Tickets", "TicketLot_TicketLotId");
            DropColumn("dbo.TicketLots", "SessionId");
            DropColumn("dbo.Draws", "OneGamePrice");
        }
    }
}
