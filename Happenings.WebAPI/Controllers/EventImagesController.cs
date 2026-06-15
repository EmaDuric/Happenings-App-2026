using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Model;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventImagesController : ControllerBase
{
    private readonly IEventImageService _service;
    public EventImagesController(IEventImageService service) => _service = service;

    private int CurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public ActionResult<List<EventImageDto>> GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    public ActionResult<EventImageDto> GetById(int id)
    {
        var result = _service.GetById(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("by-event/{eventId}")]
    public ActionResult<List<EventImageDto>> GetByEvent(int eventId)
        => Ok(_service.GetByEvent(eventId));

    [HttpPost]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public ActionResult<EventImageDto> Insert(EventImageInsertRequest request)
    {
        var result = _service.Insert(request, CurrentUserId(), User.IsInRole(Roles.Admin));
        return result == null ? Forbid() : Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public ActionResult<EventImageDto> Update(int id, EventImageUpdateRequest request)
    {
        var result = _service.Update(id, request, CurrentUserId(), User.IsInRole(Roles.Admin));
        return result == null ? Forbid() : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public IActionResult Delete(int id)
    {
        return _service.Delete(id, CurrentUserId(), User.IsInRole(Roles.Admin)) ? NoContent() : Forbid();
    }
}