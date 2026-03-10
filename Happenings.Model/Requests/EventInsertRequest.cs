namespace Happenings.Model.Requests;

public class EventInsertRequest
{
	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
	public DateTime EventDate { get; set; }
    public int EventCategoryId { get; set; }

    public int OrganizerId { get; set; }
	public int LocationId { get; set; }
}
