namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130720c : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TicketSets", "Draw_DrawId", "dbo.Draws");
            DropForeignKey("dbo.TicketSets", "Owner_UserId", "dbo.Users");
            DropIndex("dbo.TicketSets", new[] { "Draw_DrawId" });
            DropIndex("dbo.TicketSets", new[] { "Owner_UserId" });
            CreateTable(
                "dbo.TicketLots",
                c => new
                    {
                        TicketLotId = c.Int(nullable: false, identity: true),
                        CreatedUtc = c.DateTime(nullable: false),
                        TotalBTC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RefundAddress = c.String(nullable: false),
                        State = c.Int(nullable: false),
                        Draw_DrawId = c.Int(nullable: false),
                        Owner_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.TicketLotId)
                .ForeignKey("dbo.Draws", t => t.Draw_DrawId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.Owner_UserId)
                .Index(t => t.Draw_DrawId)
                .Index(t => t.Owner_UserId);
            
            DropTable("dbo.TicketSets");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TicketSets",
                c => new
                    {
                        TicketSetId = c.Int(nullable: false, identity: true),
                        CreatedUtc = c.DateTime(nullable: false),
                        TotalBTC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RefundAddress = c.String(nullable: false),
                        State = c.Int(nullable: false),
                        Draw_DrawId = c.Int(nullable: false),
                        Owner_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.TicketSetId);
            
            DropIndex("dbo.TicketLots", new[] { "Owner_UserId" });
            DropIndex("dbo.TicketLots", new[] { "Draw_DrawId" });
            DropForeignKey("dbo.TicketLots", "Owner_UserId", "dbo.Users");
            DropForeignKey("dbo.TicketLots", "Draw_DrawId", "dbo.Draws");
            DropTable("dbo.TicketLots");
            CreateIndex("dbo.TicketSets", "Owner_UserId");
            CreateIndex("dbo.TicketSets", "Draw_DrawId");
            AddForeignKey("dbo.TicketSets", "Owner_UserId", "dbo.Users", "UserId");
            AddForeignKey("dbo.TicketSets", "Draw_DrawId", "dbo.Draws", "DrawId", cascadeDelete: true);
        }
    }
}
