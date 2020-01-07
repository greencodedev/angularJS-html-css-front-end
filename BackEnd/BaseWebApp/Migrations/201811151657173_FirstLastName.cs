namespace BaseWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FirstLastName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.idty_users", "FirstName", c => c.String(unicode: false));
            AddColumn("dbo.idty_users", "LastName", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.idty_users", "LastName");
            DropColumn("dbo.idty_users", "FirstName");
        }
    }
}
