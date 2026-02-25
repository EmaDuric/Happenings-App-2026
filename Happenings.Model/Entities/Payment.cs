namespace Happenings.Model.Entities;

public class Payment
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!; // PayPal, Card

    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

    public string TransactionId { get; set; } = Guid.NewGuid().ToString();

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
