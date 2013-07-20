namespace _555Lottery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130720a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketSets", "State", c => c.Int(nullable: false));
            AddColumn("dbo.TicketSets", "Draw_DrawId", c => c.Int(nullable: false));
            AddForeignKey("dbo.TicketSets", "Draw_DrawId", "dbo.Draws", "DrawId", cascadeDelete: true);
            CreateIndex("dbo.TicketSets", "Draw_DrawId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TicketSets", new[] { "Draw_DrawId" });
            DropForeignKey("dbo.TicketSets", "Draw_DrawId", "dbo.Draws");
            DropColumn("dbo.TicketSets", "Draw_DrawId");
            DropColumn("dbo.TicketSets", "State");
        }
    }
}
