using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Happenings.Model.Messaging;
using Happenings.Services.Database;
using Happenings.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Subscriber;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection _connection;
    private IModel _channel;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "paymentQueue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🚀 Subscriber started...");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                Console.WriteLine("📥 PAYMENT MESSAGE RECEIVED:");
                Console.WriteLine(json);

                var message = JsonSerializer.Deserialize<PaymentCreatedMessage>(json);

                if (message == null)
                {
                    Console.WriteLine("❌ Invalid message format");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HappeningsContext>();

                // 🔔 NOTIFICATION
                var notification = new Notification
                {
                    UserId = message.UserId,
                    Title = "Payment Successful",
                    Message = $"Payment of {message.Amount} completed.",
                    CreatedAt = DateTime.UtcNow
                };

                context.Notifications.Add(notification);

                // 🎟️ GET RESERVATION
                var reservation = context.Reservations
                    .Include(r => r.Event)
                    .FirstOrDefault(r => r.Id == message.ReservationId);

                if (reservation == null)
                {
                    Console.WriteLine("❌ Reservation not found!");
                    return;
                }

                // 🎟️ CREATE TICKETS
                for (int i = 0; i < reservation.Quantity; i++)
                {
                    var ticket = new Ticket
                    {
                        ReservationId = reservation.Id,
                        GeneratedAt = DateTime.UtcNow,
                        QRCode = $"ticket-{reservation.Id}-{Guid.NewGuid()}", // 🔥 KLJUČNO
                        IsUsed = false                                       // 🔥 KLJUČNO
                    };

                    context.Tickets.Add(ticket);
                }

                Console.WriteLine($"🎟️ Created {reservation.Quantity} ticket(s)");

                // ✅ UPDATE STATUS
                reservation.Status = Happenings.Model.Enums.ReservationStatus.Approved;

                context.SaveChanges();

                Console.WriteLine("✅ DB changes saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR IN SUBSCRIBER:");
                Console.WriteLine(ex.Message);
            }
        };

        _channel.BasicConsume(
            queue: "paymentQueue",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }
}