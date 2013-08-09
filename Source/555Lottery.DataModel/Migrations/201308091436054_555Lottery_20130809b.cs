namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130809b : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tickets", "NumberOfGames");
            DropColumn("dbo.Tickets", "Price");
            DropColumn("dbo.Tickets", "Winnings");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tickets", "Winnings", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Tickets", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Tickets", "NumberOfGames", c => c.Int(nullable: false));
        }
    }
}
