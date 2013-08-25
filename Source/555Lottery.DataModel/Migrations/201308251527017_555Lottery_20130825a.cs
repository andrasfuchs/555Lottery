namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20130825a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Draws", "OneGameBTC", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Tickets", "SequenceHash", c => c.Binary(nullable: false));
            AddColumn("dbo.Games", "SequenceHash", c => c.Binary(nullable: false));
            AddColumn("dbo.Games", "WinningsBTC", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.Draws", "OneGamePrice");
            DropColumn("dbo.Games", "Winnings");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Games", "Winnings", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Draws", "OneGamePrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Games", "WinningsBTC");
            DropColumn("dbo.Games", "SequenceHash");
            DropColumn("dbo.Tickets", "SequenceHash");
            DropColumn("dbo.Draws", "OneGameBTC");
        }
    }
}
