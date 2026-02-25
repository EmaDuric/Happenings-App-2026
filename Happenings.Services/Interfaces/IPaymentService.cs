using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Model.Responses;


public interface IPaymentService
{
    PaymentDto Insert(PaymentInsertRequest request);
    List<PaymentDto> Get();
}
