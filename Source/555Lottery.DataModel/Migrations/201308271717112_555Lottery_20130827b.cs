namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130827b : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TicketLots", "SecondChanceParticipant", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TicketLots", "SecondChanceParticipant");
        }
    }
}
