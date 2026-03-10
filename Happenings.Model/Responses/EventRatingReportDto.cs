public class EventRatingReportDto
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public double AverageRating { get; set; }
}