namespace BaseWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.idty_roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Name = c.String(nullable: false, maxLength: 255, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.idty_user_roles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        RoleId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        IdentityUser_Id = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.idty_roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.idty_users", t => t.IdentityUser_Id)
                .Index(t => t.RoleId)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.idty_users",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Email = c.String(maxLength: 256, storeType: "nvarchar"),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(unicode: false),
                        SecurityStamp = c.String(unicode: false),
                        PhoneNumber = c.String(unicode: false),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(precision: 0),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 255, storeType: "nvarchar"),
                        Initials = c.String(unicode: false),
                        Active = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.idty_user_claims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(unicode: false),
                        ClaimType = c.String(unicode: false),
                        ClaimValue = c.String(unicode: false),
                        IdentityUser_Id = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.idty_users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.idty_user_logins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 255, storeType: "nvarchar"),
                        ProviderKey = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        IdentityUser_Id = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.idty_users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.idty_user_roles", "IdentityUser_Id", "dbo.idty_users");
            DropForeignKey("dbo.idty_user_logins", "IdentityUser_Id", "dbo.idty_users");
            DropForeignKey("dbo.idty_user_claims", "IdentityUser_Id", "dbo.idty_users");
            DropForeignKey("dbo.idty_user_roles", "RoleId", "dbo.idty_roles");
            DropIndex("dbo.idty_user_logins", new[] { "IdentityUser_Id" });
            DropIndex("dbo.idty_user_claims", new[] { "IdentityUser_Id" });
            DropIndex("dbo.idty_users", "UserNameIndex");
            DropIndex("dbo.idty_user_roles", new[] { "IdentityUser_Id" });
            DropIndex("dbo.idty_user_roles", new[] { "RoleId" });
            DropIndex("dbo.idty_roles", "RoleNameIndex");
            DropTable("dbo.idty_user_logins");
            DropTable("dbo.idty_user_claims");
            DropTable("dbo.idty_users");
            DropTable("dbo.idty_user_roles");
            DropTable("dbo.idty_roles");
        }
    }
}
