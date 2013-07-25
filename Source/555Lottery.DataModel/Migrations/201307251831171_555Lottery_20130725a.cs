namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130725a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "OneGamePrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Draws", "WinningTicketHash", c => c.Binary());
            AddColumn("dbo.Draws", "WinningTicketGeneratedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.TicketLots", "SessionId", c => c.String());
            AddColumn("dbo.Tickets", "TicketLot_TicketLotId", c => c.Int(nullable: false));
            AlterColumn("dbo.TicketLots", "RefundAddress", c => c.String());
            AddForeignKey("dbo.Tickets", "TicketLot_TicketLotId", "dbo.TicketLots", "TicketLotId", cascadeDelete: true);
            CreateIndex("dbo.Tickets", "TicketLot_TicketLotId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tickets", new[] { "TicketLot_TicketLotId" });
            DropForeignKey("dbo.Tickets", "TicketLot_TicketLotId", "dbo.TicketLots");
            AlterColumn("dbo.TicketLots", "RefundAddress", c => c.String(nullable: false));
            DropColumn("dbo.Tickets", "TicketLot_TicketLotId");
            DropColumn("dbo.TicketLots", "SessionId");
            DropColumn("dbo.Draws", "WinningTicketGeneratedAt");
            DropColumn("dbo.Draws", "WinningTicketHash");
            DropColumn("dbo.Draws", "OneGamePrice");
        }
    }
}
