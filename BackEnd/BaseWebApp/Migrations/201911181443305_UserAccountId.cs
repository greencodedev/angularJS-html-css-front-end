namespace BaseWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserAccountId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.idty_users", "AccountId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.idty_users", "AccountId");
        }
    }
}
