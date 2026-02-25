using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.DTOs;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
        => Ok(_service.Get());

    [HttpPost]
    public IActionResult Insert(PaymentInsertRequest request)
        => Ok(_service.Insert(request));
}
