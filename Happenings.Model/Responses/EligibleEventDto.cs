namespace Happenings.Model.Responses;

public class EligibleEventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string? ImageUrl { get; set; }
    public int? ExistingReviewId { get; set; }
    public int? ExistingRating { get; set; }
    public string? ExistingComment { get; set; }
}