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

    public int Quantity { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    // AUDIT TRAG � Approve
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }

    // AUDIT TRAG � Reject
    public DateTime? RejectedAt { get; set; }
    public int? RejectedByUserId { get; set; }
    public string? RejectedReason { get; set; }

    // AUDIT TRAG � Cancel
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    // AUDIT TRAG � Complete
    public DateTime? CompletedAt { get; set; }
    public int? CompletedByUserId { get; set; }
}