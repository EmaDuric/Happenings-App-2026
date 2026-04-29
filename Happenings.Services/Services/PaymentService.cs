using Happenings.Model.DTOs;
using Happenings.Model.Entities;
using Happenings.Model.Messaging;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

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

    public PaymentDto ConfirmPayment(int reservationId, string method)
    {
        // 🔥 VALIDACIJA METODE
        if (method != "Card" && method != "PayPal")
            throw new Exception("Invalid payment method");

        // 🔥 POSTOJEĆI PAYMENT (idempotent + zaštita)
        var existing = _context.Payments
            .FirstOrDefault(p => p.ReservationId == reservationId);

        // ✔ IDEMPOTENTNOST
        if (existing != null && existing.Status == "Completed")
            return MapToDto(existing);

        // ✔ SPRIJEČI DUPOLO PLAĆANJE
        if (existing != null && existing.Status == "Pending")
            throw new Exception("Payment already in progress");

        // 🔥 REZERVACIJA + TICKET TYPE
        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId);

        if (reservation == null)
            throw new Exception("Reservation not found");

        if (reservation.EventTicketType == null)
            throw new Exception("Ticket type not found");

        // 🔥 SERVER RAČUNA CIJENU
        var amount = reservation.EventTicketType.Price * reservation.Quantity;

        var entity = new Payment
        {
            ReservationId = reservationId,
            Amount = amount,
            PaymentMethod = method,
            Status = "Completed",
            PaymentDate = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString()
        };

        // 🔥 TRANSAKCIJA (DB + RabbitMQ)
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            _context.Payments.Add(entity);
            _context.SaveChanges();

            PublishPaymentEvent(entity, reservation.UserId);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return MapToDto(entity);
    }

    private PaymentDto MapToDto(Payment x)
    {
        return new PaymentDto
        {
            Id = x.Id,
            ReservationId = x.ReservationId,
            Amount = x.Amount,
            PaymentMethod = x.PaymentMethod,
            Status = x.Status,
            TransactionId = x.TransactionId,
            PaymentDate = x.PaymentDate
        };
    }

    private void PublishPaymentEvent(Payment entity, int userId)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "paymentQueue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = new PaymentCreatedMessage
        {
            ReservationId = entity.ReservationId,
            UserId = userId,
            Amount = entity.Amount
        };

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: "",
            routingKey: "paymentQueue",
            basicProperties: null,
            body: body);
    }
}