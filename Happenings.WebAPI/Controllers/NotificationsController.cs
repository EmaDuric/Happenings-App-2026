using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<List<NotificationDto>> GetAll() => Ok(_service.GetAll());

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public ActionResult<List<NotificationDto>> GetPending() => Ok(_service.GetPending());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<NotificationDto> Insert(NotificationInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}/mark-sent")]
    [Authorize(Roles = "Admin")]
    public ActionResult<NotificationDto> MarkAsSent(int id)
    {
        var result = _service.MarkAsSent(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("my")]
    public ActionResult<List<NotificationDto>> GetMy()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        return Ok(_service.GetByUserId(int.Parse(userIdClaim.Value)));
    }

    [HttpDelete("my/clear")]
    public IActionResult ClearMy()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        _service.ClearByUserId(int.Parse(userIdClaim.Value));
        return NoContent();
    }
}