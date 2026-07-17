using CLDV1.Models;
using CLDVP1.Models;
using Microsoft.EntityFrameworkCore;

namespace CLDVP1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Booking> Booking { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Venue> Venue { get; set; }

        public DbSet<EventType> EventTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly configure relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Booking)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Booking)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventType>().HasData(
        new EventType { EventTypeId = 1, TypeName = "Conference" },
        new EventType { EventTypeId = 2, TypeName = "Social" },
        new EventType { EventTypeId = 3, TypeName = "Wedding" },
        new EventType { EventTypeId = 4, TypeName = "Sports" },
        new EventType { EventTypeId = 5, TypeName = "Concert" }
    );
        }
    }
}