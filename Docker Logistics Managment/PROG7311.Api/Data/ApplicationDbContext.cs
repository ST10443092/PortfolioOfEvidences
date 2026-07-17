using Microsoft.EntityFrameworkCore;
using PROG7311.Api.Models;

namespace PROG7311.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(c => c.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Client)
                .WithMany()
                .HasForeignKey(sr => sr.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Client)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ServiceRequest>(e =>
            {
                e.Property(s => s.Cost).HasPrecision(18, 2);
                e.Property(s => s.CostInZar).HasPrecision(18, 2);
                e.Property(s => s.ExchangeRateToZar).HasPrecision(18, 6);
            });

            modelBuilder.Entity<Users>().HasData(
                new Users
                {
                    UsersId = 1,
                    Username = "client",
                    Password = "client",
                    Role = "Client",
                    ClientId = null
                },
                new Users
                {
                    UsersId = 2,
                    Username = "admin",
                    Password = "admin",
                    Role = "Admin",
                    ClientId = null
                }
            );
        }
    }
}