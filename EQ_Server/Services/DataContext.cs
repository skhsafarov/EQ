using EQ_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EQ_Server.Services
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(x => x.Role).HasDefaultValue("User");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));

            //Users.AddRange(
            //    new User("Sardor", "Safarov", "+79773864255", "admin"),
            //    new User("Leonid", null, "+71234567890", "user")
            //    );
            //SaveChanges();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Signin> Signins { get; set; }
        public DbSet<Queue> Queues { get; set; }
    }
}