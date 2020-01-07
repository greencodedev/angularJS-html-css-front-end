namespace BaseWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSuperAdmin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.idty_users", "SuperAdmin", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.idty_users", "SuperAdmin");
        }
    }
}
