public class ReservationDto
{
    public int Id { get; set; }

    public DateTime ReservedAt { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public int EventTicketTypeId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;
}