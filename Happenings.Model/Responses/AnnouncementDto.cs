public class AnnouncementResponse
{
	public int Id { get; set; }

	public int EventId { get; set; }
	public string EventName { get; set; }

	public string Title { get; set; }
	public string Content { get; set; }

	public DateTime CreatedAt { get; set; }
}