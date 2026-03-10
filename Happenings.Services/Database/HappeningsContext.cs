using Microsoft.EntityFrameworkCore;
using Happenings.Model.Entities;

namespace Happenings.Services.Database
{
    public class HappeningsContext : DbContext
    {
        public HappeningsContext(DbContextOptions<HappeningsContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organizer> Organizers => Set<Organizer>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<EventView> EventViews => Set<EventView>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<EventImage> EventImages => Set<EventImage>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<EventTicketType> EventTicketType { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Event ↔ Organizer
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event ↔ Location
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation ↔ User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            // Reservation ↔ Event
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EventId);

            // Review ↔ User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            // Review ↔ Event
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EventId);

            // EventImage ↔ Event
            modelBuilder.Entity<EventImage>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Images)
                .HasForeignKey(i => i.EventId);

            // EventCategoriy

            modelBuilder.Entity<EventCategory>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventTicketType>()
                .HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.EventTicketType)
                .WithMany()
                .HasForeignKey(r => r.EventTicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
