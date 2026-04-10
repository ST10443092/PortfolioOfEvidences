using Microsoft.EntityFrameworkCore;
using PROGP2.Models; 

namespace PROGP2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define your tables
        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relationship: 1 User ? many Claims
            modelBuilder.Entity<User>()
                .HasMany(u => u.Claims)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Explicitly configure DateCreated column for Claims to ensure proper mapping
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.Property(e => e.DateCreated)
                    .HasColumnName("DateCreated");
            });

            // Seed data
            modelBuilder.Entity<User>().HasData(
                  new User { UserID = 1, UserName = "john.doe", Password = "password123", Role = "Employee", Name = "John Doe" },
                  new User { UserID = 2, UserName = "jane.smith", Password = "password123", Role = "Employee", Name = "Jane Smith" },
                  new User { UserID = 3, UserName = "mike.johnson", Password = "password123", Role = "Manager", Name = "Mike Johnson" },
                   new User { UserID = 6, UserName = "james.sutton", Password = "HR234", Role = "HR", Name = "James Sutton" },
                  new User { UserID = 4, UserName = "sarah.wilson", Password = "password123", Role = "Employee", Name = "Sarah Wilson" },
                  new User { UserID = 5, UserName = "coordinator", Password = "coord123", Role = "Coordinator", Name = "Programme Coordinator" }
              );


        }
    }
}
