using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    // 🔥 GET (nije obavezno ali može ostati)
    [HttpGet]
    public IActionResult Get()
        => Ok(_service.Get());

    // 🔥 GLAVNI ENDPOINT ZA PLAĆANJE
    [HttpPost("confirm")]
    [Authorize]
    public IActionResult Confirm(PaymentRequest request)
    {
        Console.WriteLine($"🔥 CONFIRM HIT: reservationId={request.ReservationId}, method={request.PaymentMethod}");

        try
        {
            var result = _service.ConfirmPayment(
                request.ReservationId,
                request.PaymentMethod
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}