namespace Happenings.Model.Responses;

public class EventViewDto
{
	public int Id { get; set; }
	public DateTime ViewedAt { get; set; }

	public int UserId { get; set; }
	public int EventId { get; set; }
}
