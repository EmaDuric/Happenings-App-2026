namespace Happenings.Model.Responses;

public class EventImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public int EventId { get; set; }
}
