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
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var message = JsonSerializer.Deserialize<PaymentCreatedMessage>(json);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HappeningsContext>();

            var notification = new Notification
            {
                UserId = message!.UserId,
                Title = "Payment Successful",
                Message = $"Payment of {message.Amount} completed.",
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            context.SaveChanges();
        };

        _channel.BasicConsume(
            queue: "paymentQueue",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }
}
