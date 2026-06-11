namespace Happenings.Model.Entities;

public class EventView
{
    public int Id { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
}
