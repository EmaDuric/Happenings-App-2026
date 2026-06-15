using Happenings.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventViewsController : ControllerBase
{
    private readonly IEventViewService _service;
    public EventViewsController(IEventViewService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<List<EventViewDto>> GetAll() => Ok(_service.GetAll());

    [HttpGet("by-event/{eventId}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public ActionResult<List<EventViewDto>> GetByEvent(int eventId)
        => Ok(_service.GetByEvent(eventId));

    // Korisnik vidi svoje preglede — userId iz JWT tokena
    [HttpGet("my")]
    public ActionResult<List<EventViewDto>> GetMy()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetByUser(userId));
    }

    // Admin može dohvatiti preglede za bilo kojeg korisnika
    [HttpGet("by-user/{userId}")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<List<EventViewDto>> GetByUser(int userId)
        => Ok(_service.GetByUser(userId));

    // userId se uzima iz JWT tokena — ne iz requesta
    [HttpPost]
    public ActionResult<EventViewDto> Insert([FromBody] EventViewInsertRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.Insert(request, userId));
    }
}