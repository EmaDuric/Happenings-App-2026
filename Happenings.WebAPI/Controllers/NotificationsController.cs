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
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetByUserId(userId));
    }

    // Mark single notification as read
    [HttpPut("{id}/mark-read")]
    public IActionResult MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = _service.MarkAsRead(id, userId);
        if (!result) return NotFound();
        return Ok();
    }

    // Mark all as read
    [HttpPut("my/mark-all-read")]
    public IActionResult MarkAllAsRead()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _service.MarkAllAsRead(userId);
        return Ok();
    }

    [HttpDelete("my/clear")]
    public IActionResult ClearMy()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _service.ClearByUserId(userId);
        return NoContent();
    }
}