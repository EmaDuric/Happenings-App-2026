using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventViewsController : ControllerBase
{
    private readonly IEventViewService _service;

    public EventViewsController(IEventViewService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<EventViewDto>> GetAll()
        => Ok(_service.GetAll());

    [HttpGet("by-event/{eventId}")]
    public ActionResult<List<EventViewDto>> GetByEvent(int eventId)
        => Ok(_service.GetByEvent(eventId));

    [HttpGet("by-user/{userId}")]
    public ActionResult<List<EventViewDto>> GetByUser(int userId)
        => Ok(_service.GetByUser(userId));

    [HttpPost]
    public ActionResult<EventViewDto> Insert(EventViewInsertRequest request)
        => Ok(_service.Insert(request));
}
