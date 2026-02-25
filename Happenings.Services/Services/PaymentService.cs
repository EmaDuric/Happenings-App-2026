using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PaymentService : IPaymentService
{
    private readonly HappeningsContext _context;

    public PaymentService(HappeningsContext context)
    {
        _context = context;
    }

    public List<PaymentDto> Get()
    {
        return _context.Payments
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                ReservationId = x.ReservationId,
                Amount = x.Amount,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                TransactionId = x.TransactionId,
                PaymentDate = x.PaymentDate
            }).ToList();
    }

    public PaymentDto Insert(PaymentInsertRequest request)
    {
        var entity = new Payment
        {
            ReservationId = request.ReservationId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = "Completed"
        };

        _context.Payments.Add(entity);
        _context.SaveChanges();

        return new PaymentDto
        {
            Id = entity.Id,
            ReservationId = entity.ReservationId,
            Amount = entity.Amount,
            PaymentMethod = entity.PaymentMethod,
            Status = entity.Status,
            TransactionId = entity.TransactionId,
            PaymentDate = entity.PaymentDate
        };
    }
}
