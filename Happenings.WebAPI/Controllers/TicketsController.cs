using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet("my")]
    [Authorize]
    public IActionResult GetMyTickets()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        return Ok(_service.GetByUserId(userId));
    }
}
