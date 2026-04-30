using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Happenings.Model;


namespace Happenings.WebAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _service;

    public AnnouncementController(IAnnouncementService service)
    {
        _service = service;
    }

    // ORGANIZER KREIRA
    [HttpPost]
    public async Task<AnnouncementResponse> Insert(AnnouncementInsertRequest request)
    {
        return await _service.InsertAsync(request);
    }

    // ADMIN / ORGANIZER VIEW
    [HttpGet("event/{eventId}")]
    public async Task<List<AnnouncementResponse>> GetForEvent(int eventId)
    {
        return await _service.GetForEventAsync(eventId);
    }

    // 🔥 USER VIEW
    [HttpGet("user/{userId}")]
    public async Task<List<AnnouncementResponse>> GetForUser(int userId)
    {
        return await _service.GetForUserAsync(userId);
    }
}