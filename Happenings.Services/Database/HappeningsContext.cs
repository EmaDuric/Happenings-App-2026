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
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<EventTicketType> EventTicketTypes => Set<EventTicketType>(); // 🔥 ime ispravljeno


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 🔥 1:1 USER ↔ ORGANIZER
            modelBuilder.Entity<Organizer>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organizer)
                .HasForeignKey<Organizer>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organizer>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            // EVENT ↔ ORGANIZER
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // EVENT ↔ LOCATION
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // EVENT ↔ CATEGORY
            modelBuilder.Entity<EventCategory>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // RESERVATION ↔ USER
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            // RESERVATION ↔ EVENT
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EventId);

            // REVIEW ↔ USER
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            // REVIEW ↔ EVENT
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EventId);

            // EVENT IMAGE
            modelBuilder.Entity<EventImage>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Images)
                .HasForeignKey(i => i.EventId);

            // EVENT TICKET TYPE
            modelBuilder.Entity<EventTicketType>()
        .ToTable("EventTicketType"); // 🔥 OVO MORA BITI POSEBNO

            modelBuilder.Entity<EventTicketType>()
                .HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // RESERVATION ↔ EVENT TICKET TYPE
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.EventTicketType)
                .WithMany()
                .HasForeignKey(r => r.EventTicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}