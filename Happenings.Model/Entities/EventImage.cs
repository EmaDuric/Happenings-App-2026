namespace Happenings.Model.Entities;

public class EventImage
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
}
