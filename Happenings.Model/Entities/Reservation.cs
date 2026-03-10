using Happenings.Model.Enums;

namespace Happenings.Model.Entities;

public class Reservation
{
    public int Id { get; set; }

    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int EventTicketTypeId { get; set; }
    public EventTicketType EventTicketType { get; set; } = null!;

    // koliko karata korisnik rezerviše
    public int Quantity { get; set; }

    // status rezervacije
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
}