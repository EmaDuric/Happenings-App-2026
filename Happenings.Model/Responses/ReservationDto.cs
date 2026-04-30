public class ReservationDto
{
    public int Id { get; set; }
    public DateTime ReservedAt { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int EventTicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
}