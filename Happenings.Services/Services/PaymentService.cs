using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Happenings.Model.Messaging;



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
        var reservation = _context.Reservations
            .First(x => x.Id == request.ReservationId);

        var entity = new Payment
        {
            ReservationId = request.ReservationId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = "Completed"
        };

        _context.Payments.Add(entity);
        _context.SaveChanges();

        // 🔥 RabbitMQ publishing
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "paymentQueue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = new PaymentCreatedMessage
        {
            ReservationId = entity.ReservationId,
            UserId = reservation.UserId,
            Amount = entity.Amount
        };

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: "",
            routingKey: "paymentQueue",
            basicProperties: null,
            body: body);

        // 🔥 Return DTO
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
