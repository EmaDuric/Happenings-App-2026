namespace Happenings.Model.Messaging;

public class PaymentCreatedMessage
{
    public int ReservationId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}
