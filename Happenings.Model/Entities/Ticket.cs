namespace Happenings.Model.Entities;

public class Ticket
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public string QRCode { get; set; } = null!;

    public bool IsUsed { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public int EventId { get; set; }
    public Event Event { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }


}
