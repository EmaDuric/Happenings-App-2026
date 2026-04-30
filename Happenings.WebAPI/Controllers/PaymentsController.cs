using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;

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

    [HttpPost("confirm")]
    public IActionResult Confirm(PaymentRequest request)
    {
        try
        {
            var result = _service.ConfirmPayment(request.ReservationId, request.PaymentMethod);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}