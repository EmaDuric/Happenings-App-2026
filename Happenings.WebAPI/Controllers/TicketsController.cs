using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;
    public TicketsController(ITicketService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get() => Ok(_service.Get());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Insert(TicketInsertRequest request) => Ok(_service.Insert(request));

    [HttpGet("my")]
    public IActionResult GetMyTickets()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetByUserId(userId));
    }
}