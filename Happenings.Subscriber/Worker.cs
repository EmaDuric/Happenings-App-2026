using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Happenings.Model.Messaging;
using Happenings.Services.Database;
using Happenings.Services.Services;
using Happenings.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Happenings.Subscriber;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Čita RabbitMQ konfiguraciju iz environment varijabli
        var host = _configuration["RabbitMQ:Host"] ?? "localhost";
        var user = _configuration["RabbitMQ:User"] ?? "guest";
        var pass = _configuration["RabbitMQ:Pass"] ?? "guest";

        var factory = new ConnectionFactory()
        {
            HostName = host,
            UserName = user,
            Password = pass
        };

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "paymentQueue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                _logger.LogInformation("✅ RabbitMQ connected on attempt {Attempt}", attempt);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ RabbitMQ not ready (attempt {Attempt}/5): {Message}", attempt, ex.Message);
                await Task.Delay(3000, cancellationToken);
            }
        }

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Subscriber started...");

        if (_channel == null)
        {
            _logger.LogError("❌ Channel nije inicijaliziran!");
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;

            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                _logger.LogInformation("📥 Message received: {Json}", json);

                var message = JsonSerializer.Deserialize<PaymentCreatedMessage>(json);

                if (message == null)
                {
                    _logger.LogError("❌ Invalid message format — NACKing");
                    _channel!.BasicNack(deliveryTag, multiple: false, requeue: false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HappeningsContext>();
                var qrService = scope.ServiceProvider.GetRequiredService<QrCodeService>();

                // Dohvati rezervaciju
                var reservation = context.Reservations
                    .Include(r => r.Event)
                    .FirstOrDefault(r => r.Id == message.ReservationId);

                if (reservation == null)
                {
                    _logger.LogError("❌ Reservation {Id} not found — NACKing", message.ReservationId);
                    _channel!.BasicNack(deliveryTag, multiple: false, requeue: false);
                    return;
                }

                // Notifikacija
                context.Notifications.Add(new Notification
                {
                    UserId = message.UserId,
                    Title = "Payment Successful",
                    Message = $"Payment of {message.Amount} KM completed. Your tickets are ready.",
                    CreatedAt = DateTime.UtcNow
                });

                // Kreiraj onoliko ticketa koliko je korisnik kupio (Quantity)
                var tickets = new List<Ticket>();
                for (int i = 0; i < reservation.Quantity; i++)
                {
                    tickets.Add(new Ticket
                    {
                        ReservationId = reservation.Id,
                        EventId = reservation.EventId,
                        UserId = reservation.UserId,
                        GeneratedAt = DateTime.UtcNow,
                        QRCode = string.Empty, // popunjava se nakon dodjele Id-a
                        IsUsed = false
                    });
                }

                context.Tickets.AddRange(tickets);
                context.SaveChanges(); // dodjeljuje Id-eve ticketima

                // Isti QR servis i format (TICKET-{id} -> base64 PNG) kao TicketService.Insert
                foreach (var ticket in tickets)
                    ticket.QRCode = qrService.GenerateQRCode($"TICKET-{ticket.Id}");

                context.SaveChanges();

                _logger.LogInformation("✅ Created {Count} ticket(s) for reservation {Id}",
                    reservation.Quantity, reservation.Id);

                _channel!.BasicAck(deliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing message");
                if (ex.InnerException != null)
                    _logger.LogError("INNER: {Message}", ex.InnerException.Message);

                _channel!.BasicNack(deliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: "paymentQueue",
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}