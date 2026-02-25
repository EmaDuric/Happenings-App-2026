namespace Happenings.Model.Requests;

public class EventCategoryUpdateRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
