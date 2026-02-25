public class PaymentInsertRequest
{
    public int ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
}
