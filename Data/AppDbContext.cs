using Microsoft.EntityFrameworkCore;
using TaskMasterAPI.Models;

namespace TaskMasterAPI.Data  // Data Access Layer namespace
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Define your DbSet properties (tables) here
        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
       // Seed data (Optional)
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "hashedpassword",
                    Role = "Admin"
                },
                new User
                {
                    Id = 2,
                    Username = "user1",
                    PasswordHash = "hashedpassword",
                    Role = "User"
                }
            );

            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = 1,
                    Title = "First Task",
                    Description = "This is the first task",
                    Status = "Pending",
                    AssignedUserId = 1
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Second Task",
                    Description = "This is the second task",
                    Status = "Completed",
                    AssignedUserId = 2
                }
            );
        }
        
    }
}