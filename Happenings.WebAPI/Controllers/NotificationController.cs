using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<NotificationDto>> GetAll()
        => Ok(_service.GetAll());

    [HttpGet("pending")]
    public ActionResult<List<NotificationDto>> GetPending()
        => Ok(_service.GetPending());

    [HttpPost]
    public ActionResult<NotificationDto> Insert(NotificationInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}/mark-sent")]
    public ActionResult<NotificationDto> MarkAsSent(int id)
    {
        var result = _service.MarkAsSent(id);
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
