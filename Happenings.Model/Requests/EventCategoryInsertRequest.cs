namespace Happenings.Model.Requests;

public class EventCategoryInsertRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
