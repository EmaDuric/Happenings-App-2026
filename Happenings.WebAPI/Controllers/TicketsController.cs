using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.DTOs;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketsController(ITicketService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
        => Ok(_service.Get());

    [HttpPost]
    public IActionResult Insert(TicketInsertRequest request)
        => Ok(_service.Insert(request));
}
