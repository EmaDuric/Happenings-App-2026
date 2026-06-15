using Happenings.Model;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTicketTypeController : ControllerBase
{
    private readonly IEventTicketTypeService _service;

    public EventTicketTypeController(IEventTicketTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] int eventId) => Ok(_service.GetByEvent(eventId));

    [HttpPost]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public IActionResult Insert([FromBody] EventTicketTypeInsertRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);

        var result = _service.Insert(request, userId, isAdmin);
        return result == null ? Forbid() : Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public IActionResult Update(int id, [FromBody] EventTicketTypeInsertRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);

        var result = _service.Update(id, request, userId, isAdmin);
        return result == null ? Forbid() : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public IActionResult Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);

        return _service.Delete(id, userId, isAdmin) ? NoContent() : Forbid();
    }
}