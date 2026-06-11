using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Model.Responses;


public interface IPaymentService
{
    List<PaymentDto> Get();
    Task<string> CreatePayPalOrderAsync(int reservationId, int userId);
    Task<PaymentDto> CapturePayPalOrderAsync(string orderId, int reservationId, int userId);
    Task<string> CreateStripePaymentIntentAsync(int reservationId, int userId);
    Task<PaymentDto> ConfirmStripePaymentAsync(string paymentIntentId, int reservationId, int userId);
}
