namespace Happenings.Model.DTOs;

public class TicketDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public string QRCode { get; set; } = null!;
    public bool IsUsed { get; set; }
    public DateTime GeneratedAt { get; set; }

    public string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; }
}
