using Microsoft.EntityFrameworkCore;
using SWWebAPI.Handler;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<employee> employees { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<menu_list> menu_list { get; set; }
        public DbSet<subscribers> subscribers { get; set; }
        public DbSet<subscription_plan> subscription_plan { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            _ = modelBuilder.Entity<UserAccount>().HasData([
                new UserAccount
                {
                    Id = 1,
                    FullName = "Administrator",
                    UserName = "admin",
                    Password = PasswordHashHandler.HashPassword("admin123"),
                }
                ]);
        }
    }
}
