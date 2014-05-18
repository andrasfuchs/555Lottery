namespace _555Lottery.DataModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555Lottery_20140518a : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "OwnedAffiliateCodes", c => c.String());
            AddColumn("dbo.Users", "FirstAffiliateCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "FirstAffiliateCode");
            DropColumn("dbo.Users", "OwnedAffiliateCodes");
        }
    }
}
