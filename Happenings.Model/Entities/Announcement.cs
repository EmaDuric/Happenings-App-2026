namespace Happenings.Model.Entities;


public class Announcement
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }

    public string Title { get; set; }
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Announcement> Announcements { get; set; }
}