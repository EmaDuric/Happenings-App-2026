using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get() => Ok(_service.Get());

    // Card plaćanje
    [HttpPost("confirm")]
    public IActionResult Confirm(PaymentRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = _service.ConfirmPayment(request.ReservationId, request.PaymentMethod, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Kreira PayPal order i vraća approval URL za Flutter WebView
    [HttpPost("paypal/create-order")]
    public async Task<IActionResult> CreatePayPalOrder([FromBody] CreatePayPalOrderRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var approvalUrl = await _service.CreatePayPalOrderAsync(request.ReservationId, userId);
            return Ok(new { approvalUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Server-side capture — verifikacija i finalizacija PayPal plaćanja
    [HttpPost("paypal/capture")]
    public async Task<IActionResult> CapturePayPalOrder([FromBody] CapturePayPalOrderRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.CapturePayPalOrderAsync(request.OrderId, request.ReservationId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Dodaj ove endpointe u PaymentsController.cs

    // Stripe — kreira PaymentIntent i vraća clientSecret
    [HttpPost("stripe/create-intent")]
    public async Task<IActionResult> CreateStripeIntent([FromBody] CreatePayPalOrderRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var clientSecret = await _service.CreateStripePaymentIntentAsync(request.ReservationId, userId);
            var publishableKey = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Stripe:PublishableKey"];
            return Ok(new { clientSecret, publishableKey });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Stripe — server-side verifikacija nakon plaćanja
    [HttpPost("stripe/confirm")]
    public async Task<IActionResult> ConfirmStripePayment([FromBody] ConfirmStripePaymentRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.ConfirmStripePaymentAsync(request.PaymentIntentId, request.ReservationId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}