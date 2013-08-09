namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130809a : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Int(nullable: false, identity: true),
                        Sequence = c.String(nullable: false),
                        Winnings = c.Decimal(precision: 18, scale: 2),
                        Ticket_TicketId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GameId)
                .ForeignKey("dbo.Tickets", t => t.Ticket_TicketId, cascadeDelete: true)
                .Index(t => t.Ticket_TicketId);
            
            AddColumn("dbo.Tickets", "NumberOfGames", c => c.Int(nullable: false));
            AddColumn("dbo.Tickets", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropIndex("dbo.Games", new[] { "Ticket_TicketId" });
            DropForeignKey("dbo.Games", "Ticket_TicketId", "dbo.Tickets");
            DropColumn("dbo.Tickets", "Price");
            DropColumn("dbo.Tickets", "NumberOfGames");
            DropTable("dbo.Games");
        }
    }
}
