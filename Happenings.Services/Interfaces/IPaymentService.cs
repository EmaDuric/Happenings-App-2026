using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Model.Responses;


public interface IPaymentService
{
    List<PaymentDto> Get();

    PaymentDto ConfirmPayment(int reservationId, string method);
}
