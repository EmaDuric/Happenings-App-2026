namespace Happenings.Model.Entities;

public class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime EventDate { get; set; }

    public int OrganizerId { get; set; }
    public Organizer Organizer { get; set; } = null!;

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int EventCategoryId { get; set; }
    public EventCategory EventCategory { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<EventImage> Images { get; set; } = new List<EventImage>();
    public ICollection<EventTicketType> TicketTypes { get; set; } = new List<EventTicketType>();
    public ICollection<Announcement> Announcements { get; set; }
}

