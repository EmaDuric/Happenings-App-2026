using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventImagesController : ControllerBase
{
    private readonly IEventImageService _service;
    public EventImagesController(IEventImageService service) => _service = service;

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
    [Authorize]
    public ActionResult<EventImageDto> Insert(EventImageInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    [Authorize]
    public ActionResult<EventImageDto> Update(int id, EventImageUpdateRequest request)
    {
        var result = _service.Update(id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}