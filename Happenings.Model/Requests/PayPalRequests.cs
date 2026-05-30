namespace Happenings.Model.Requests
{
    public class CreatePayPalOrderRequest
    {
        public int ReservationId { get; set; }
    }

    public class CapturePayPalOrderRequest
    {
        public string OrderId { get; set; } = null!;
        public int ReservationId { get; set; }
    }

    public class ConfirmStripePaymentRequest
    {
        public string PaymentIntentId { get; set; } = null!;
        public int ReservationId { get; set; }
    }
}