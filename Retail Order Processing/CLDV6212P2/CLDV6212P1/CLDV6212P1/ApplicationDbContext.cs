using CLDV6212P1.Models;
using CLDV6212P1.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;


namespace CLDV6212P1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Primary Key
                entity.HasKey(u => u.UserId);

                // Properties configuration
                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Surname)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("User");

                entity.Property(u => u.DateCreated)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()"); 

             
                entity.HasIndex(u => u.Email)
                    .IsUnique();

              
                entity.ToTable("Users");
            });

          
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Name = "Admin",
                    Surname = "System",
                    Email = "admin@cldv6212p1.com",
                    PasswordHash = PasswordHelper.HashPassword("pass123!"), 
                    Role = "Manager",
                    DateCreated = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}