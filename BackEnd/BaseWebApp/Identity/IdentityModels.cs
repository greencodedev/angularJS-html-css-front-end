using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MySql.Data.Entity;

namespace BaseWebApp.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Initials { get; set; }
        public bool Active { get; set; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public bool? SuperAdmin { get; set; }
        public int AccountId { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("idty_users").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<ApplicationUser>().Property(p => p.UserName).HasMaxLength(255);

            modelBuilder.Entity<IdentityUserRole>().ToTable("idty_user_roles").Property(p => p.RoleId).HasMaxLength(255);
            modelBuilder.Entity<IdentityUserLogin>().ToTable("idty_user_logins").Property(p => p.LoginProvider).HasMaxLength(255);
            modelBuilder.Entity<IdentityUserClaim>().ToTable("idty_user_claims");

            modelBuilder.Entity<IdentityRole>().ToTable("idty_roles");
            modelBuilder.Entity<IdentityRole>().Property(p => p.Name).HasMaxLength(255);


            modelBuilder.Entity<IdentityUser>().ToTable("idty_users");
            modelBuilder.Entity<IdentityUser>().Property(p => p.Id).HasColumnName("UserId");
        }
    }
}