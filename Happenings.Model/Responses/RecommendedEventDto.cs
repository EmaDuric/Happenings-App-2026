public class RecommendedEventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int EventCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? LocationName { get; set; }
    public double Score { get; set; }
    public string? Reason { get; set; }  // ← dodaj
    public string? ImageUrl { get; set; }
}