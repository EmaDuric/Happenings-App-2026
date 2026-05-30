using Happenings.Model.DTOs;
using Happenings.Model.Entities;
using Happenings.Model.Enums;
using Happenings.Model.Messaging;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Stripe;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class PaymentService : IPaymentService
{
    private readonly HappeningsContext _context;
    private readonly IConfiguration _configuration;

    public PaymentService(HappeningsContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;

        // Inicijaliziraj Stripe sa secret key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
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

    // Stripe — kreira PaymentIntent i vraća clientSecret za Flutter PaymentSheet
    public async Task<string> CreateStripePaymentIntentAsync(int reservationId, int userId)
    {
        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new Exception("Reservation not found");

        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException("You can only pay for your own reservation");

        if (reservation.Status != ReservationStatus.Pending)
            throw new Exception($"Reservation cannot be paid. Current status: {reservation.Status}");

        if (reservation.EventTicketType == null)
            throw new Exception("Ticket type not found");

        if (reservation.EventTicketType.AvailableQuantity < reservation.Quantity)
            throw new Exception("Not enough tickets available");

        var existing = _context.Payments.FirstOrDefault(p => p.ReservationId == reservationId);
        if (existing != null && existing.Status == "Completed")
            throw new Exception("Already paid");

        // Obrisi stari pending ako postoji
        if (existing != null && existing.Status == "Pending")
        {
            _context.Payments.Remove(existing);
            _context.SaveChanges();
        }

        // Server određuje iznos — klijent ne šalje cijenu
        var amount = reservation.EventTicketType.Price * reservation.Quantity;
        // Stripe iznos je u centima
        var amountInCents = (long)(amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "reservationId", reservationId.ToString() },
                { "userId", userId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        // Spremi pending payment sa PaymentIntent ID
        var pendingPayment = new Payment
        {
            ReservationId = reservationId,
            Amount = amount,
            PaymentMethod = "Card",
            Status = "Pending",
            TransactionId = paymentIntent.Id,
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(pendingPayment);
        _context.SaveChanges();

        return paymentIntent.ClientSecret;
    }

    // Stripe — server-side verifikacija nakon plaćanja
    public async Task<PaymentDto> ConfirmStripePaymentAsync(string paymentIntentId, int reservationId, int userId)
    {
        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new Exception("Reservation not found");

        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized");

        // Idempotentnost
        var existing = _context.Payments.FirstOrDefault(p => p.ReservationId == reservationId);
        if (existing != null && existing.Status == "Completed")
            return MapToDto(existing);

        // Server-side verifikacija — provjeri status na Stripe-u
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId);

        if (paymentIntent.Status != "succeeded")
            throw new Exception($"Payment not confirmed. Stripe status: {paymentIntent.Status}");

        // Verifikuj da iznos odgovara — klijent nije mogao manipulirati cijenom
        var expectedAmount = reservation.EventTicketType!.Price * reservation.Quantity;
        var stripeAmount = paymentIntent.Amount / 100m;

        if (Math.Abs(stripeAmount - expectedAmount) > 0.01m)
            throw new Exception("Payment amount mismatch");

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (existing != null)
            {
                existing.Status = "Completed";
                existing.PaymentDate = DateTime.UtcNow;
            }
            else
            {
                existing = new Payment
                {
                    ReservationId = reservationId,
                    Amount = expectedAmount,
                    PaymentMethod = "Card",
                    Status = "Completed",
                    TransactionId = paymentIntentId,
                    PaymentDate = DateTime.UtcNow
                };
                _context.Payments.Add(existing);
            }

            reservation.EventTicketType.AvailableQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Approved;
            _context.SaveChanges();
            PublishPaymentEvent(existing, reservation.UserId);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return MapToDto(existing);
    }

    public async Task<string> CreatePayPalOrderAsync(int reservationId, int userId)
    {
        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new Exception("Reservation not found");

        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException("You can only pay for your own reservation");

        if (reservation.Status != ReservationStatus.Pending)
            throw new Exception($"Reservation cannot be paid. Current status: {reservation.Status}");

        if (reservation.EventTicketType == null)
            throw new Exception("Ticket type not found");

        if (reservation.EventTicketType.AvailableQuantity < reservation.Quantity)
            throw new Exception("Not enough tickets available");

        var existing = _context.Payments.FirstOrDefault(p => p.ReservationId == reservationId);
        if (existing != null && existing.Status == "Completed")
            throw new Exception("Already paid");

        if (existing != null && existing.Status == "Pending")
        {
            _context.Payments.Remove(existing);
            _context.SaveChanges();
        }

        var amount = reservation.EventTicketType.Price * reservation.Quantity;
        var accessToken = await GetPayPalAccessTokenAsync();

        var paypalBaseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
        var returnUrl = _configuration["PayPal:ReturnUrl"] ?? "https://happenings.app/payment/success";
        var cancelUrl = _configuration["PayPal:CancelUrl"] ?? "https://happenings.app/payment/cancel";

        var orderRequest = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = reservationId.ToString(),
                    amount = new
                    {
                        currency_code = "USD",
                        value = amount.ToString("F2", CultureInfo.InvariantCulture)
                    },
                    description = $"Happenings ticket - Reservation #{reservationId}"
                }
            },
            application_context = new
            {
                return_url = $"{returnUrl}?reservationId={reservationId}",
                cancel_url = $"{cancelUrl}?reservationId={reservationId}",
                user_action = "PAY_NOW"
            }
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = new StringContent(JsonSerializer.Serialize(orderRequest), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{paypalBaseUrl}/v2/checkout/orders", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"PayPal order creation failed: {responseBody}");

        var orderData = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var orderId = orderData.GetProperty("id").GetString();

        var pendingPayment = new Payment
        {
            ReservationId = reservationId,
            Amount = amount,
            PaymentMethod = "PayPal",
            Status = "Pending",
            TransactionId = orderId,
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(pendingPayment);
        _context.SaveChanges();

        var links = orderData.GetProperty("links");
        foreach (var link in links.EnumerateArray())
        {
            if (link.GetProperty("rel").GetString() == "approve")
                return link.GetProperty("href").GetString()!;
        }

        throw new Exception("PayPal approval URL not found");
    }

    public async Task<PaymentDto> CapturePayPalOrderAsync(string orderId, int reservationId, int userId)
    {
        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new Exception("Reservation not found");

        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized");

        var existing = _context.Payments.FirstOrDefault(p => p.ReservationId == reservationId);
        if (existing != null && existing.Status == "Completed")
            return MapToDto(existing);

        var accessToken = await GetPayPalAccessTokenAsync();
        var paypalBaseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.PostAsync(
            $"{paypalBaseUrl}/v2/checkout/orders/{orderId}/capture",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"PayPal capture failed: {responseBody}");

        var captureData = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var status = captureData.GetProperty("status").GetString();

        if (status != "COMPLETED")
            throw new Exception($"PayPal payment not completed. Status: {status}");

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            if (existing != null)
            {
                existing.Status = "Completed";
                existing.PaymentDate = DateTime.UtcNow;
            }
            else
            {
                var amount = reservation.EventTicketType!.Price * reservation.Quantity;
                existing = new Payment
                {
                    ReservationId = reservationId,
                    Amount = amount,
                    PaymentMethod = "PayPal",
                    Status = "Completed",
                    TransactionId = orderId,
                    PaymentDate = DateTime.UtcNow
                };
                _context.Payments.Add(existing);
            }

            reservation.EventTicketType!.AvailableQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Approved;
            _context.SaveChanges();
            PublishPaymentEvent(existing, reservation.UserId);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return MapToDto(existing);
    }

    public PaymentDto ConfirmPayment(int reservationId, string method, int userId)
    {
        if (method != "Card" && method != "PayPal")
            throw new Exception("Invalid payment method. Use 'Card' or 'PayPal'");

        var reservation = _context.Reservations
            .Include(r => r.EventTicketType)
            .FirstOrDefault(r => r.Id == reservationId)
            ?? throw new Exception("Reservation not found");

        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException("You can only pay for your own reservation");

        if (reservation.Status != ReservationStatus.Pending)
            throw new Exception($"Reservation cannot be paid. Current status: {reservation.Status}");

        if (reservation.EventTicketType == null)
            throw new Exception("Ticket type not found");

        if (reservation.EventTicketType.AvailableQuantity < reservation.Quantity)
            throw new Exception("Not enough tickets available");

        var existing = _context.Payments.FirstOrDefault(p => p.ReservationId == reservationId);
        if (existing != null && existing.Status == "Completed")
            return MapToDto(existing);
        if (existing != null && existing.Status == "Pending")
            throw new Exception("Payment already in progress");

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

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            reservation.EventTicketType.AvailableQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Approved;
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

    private async Task<string> GetPayPalAccessTokenAsync()
    {
        var clientId = _configuration["PayPal:ClientId"];
        var secret = _configuration["PayPal:Secret"];
        var paypalBaseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(secret))
            throw new Exception("PayPal credentials not configured");

        using var authClient = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await authClient.PostAsync($"{paypalBaseUrl}/v1/oauth2/token", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"PayPal auth failed: {responseBody}");

        var tokenData = JsonSerializer.Deserialize<JsonElement>(responseBody);
        return tokenData.GetProperty("access_token").GetString()!;
    }

    private PaymentDto MapToDto(Payment x) => new PaymentDto
    {
        Id = x.Id,
        ReservationId = x.ReservationId,
        Amount = x.Amount,
        PaymentMethod = x.PaymentMethod,
        Status = x.Status,
        TransactionId = x.TransactionId,
        PaymentDate = x.PaymentDate
    };

    private void PublishPaymentEvent(Payment entity, int userId)
    {
        var host = _configuration["RabbitMQ:Host"] ?? "localhost";
        var user = _configuration["RabbitMQ:User"] ?? "guest";
        var pass = _configuration["RabbitMQ:Pass"] ?? "guest";

        var factory = new ConnectionFactory() { HostName = host, UserName = user, Password = pass };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "paymentQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var message = new PaymentCreatedMessage
        {
            ReservationId = entity.ReservationId,
            UserId = userId,
            Amount = entity.Amount
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "", routingKey: "paymentQueue", basicProperties: null, body: body);
    }
}