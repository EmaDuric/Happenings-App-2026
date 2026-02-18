namespace Happenings.Model.Requests;

public class EventImageInsertRequest
{
    public string ImageUrl { get; set; } = null!;
    public int EventId { get; set; }
}
