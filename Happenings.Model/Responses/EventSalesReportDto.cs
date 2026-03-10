public class EventSalesReportDto
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public int TicketsSold { get; set; }
}