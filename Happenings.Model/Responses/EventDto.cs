namespace Happenings.Model.Responses;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public int EventCategoryId { get; set; }
    public int OrganizerId { get; set; }
    public int LocationId { get; set; }
    public string? LocationName { get; set; }

    public string? CategoryName { get; set; }
}
