namespace Happenings.Model.Responses;

public class EventPopularityDto
{
    public int EventId { get; set; }

    public string EventName { get; set; }

    public int TotalReservations { get; set; }
}