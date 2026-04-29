namespace Happenings.Model.Requests
{
    public class PaymentRequest
    {
        public int ReservationId { get; set; }
        public string PaymentMethod { get; set; } = "";
    }
}