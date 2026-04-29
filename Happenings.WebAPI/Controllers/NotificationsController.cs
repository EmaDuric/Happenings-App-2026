using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet("my")]
    [Authorize]
    public ActionResult<List<NotificationDto>> GetMy()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        return Ok(_service.GetByUserId(userId));
    }

    [HttpDelete("my/clear")]
    [Authorize]
    public IActionResult ClearMy()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        _service.ClearByUserId(userId);
        return NoContent();
    }
}
