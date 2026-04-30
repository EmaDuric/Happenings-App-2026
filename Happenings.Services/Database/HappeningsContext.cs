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
        public DbSet<EventTicketType> EventTicketTypes => Set<EventTicketType>();
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

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

            // USER ↔ ORGANIZER (1:1)
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

            // EVENT TICKET TYPE
            modelBuilder.Entity<EventTicketType>()
                .ToTable("EventTicketType");

            modelBuilder.Entity<EventTicketType>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventTicketType>()
                .HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EVENT IMAGE
            modelBuilder.Entity<EventImage>()
                .HasOne(i => i.Event)
                .WithMany(e => e.Images)
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // RESERVATION ↔ USER
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RESERVATION ↔ EVENT (single, authoritative definition)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // RESERVATION ↔ EVENT TICKET TYPE
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.EventTicketType)
                .WithMany()
                .HasForeignKey(r => r.EventTicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // TICKET ↔ RESERVATION
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Reservation)
                .WithMany()
                .HasForeignKey(t => t.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // TICKET ↔ EVENT
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Event)
                .WithMany()
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // TICKET ↔ USER
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PAYMENT ↔ RESERVATION
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany()
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // REVIEW ↔ USER
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // REVIEW ↔ EVENT
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Reviews)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // INVITATION
            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Event)
                .WithMany()
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Sender)
                .WithMany()
                .HasForeignKey(i => i.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Receiver)
                .WithMany()
                .HasForeignKey(i => i.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // ANNOUNCEMENT ↔ EVENT
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Event)
                .WithMany(e => e.Announcements)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}