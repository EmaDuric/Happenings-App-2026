using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Model;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _service;
    public AnnouncementController(IAnnouncementService service) => _service = service;

    [HttpPost]
    public async Task<AnnouncementResponse> Insert(AnnouncementInsertRequest request)
        => await _service.InsertAsync(request);

    [HttpGet("event/{eventId}")]
    public async Task<List<AnnouncementResponse>> GetForEvent(int eventId)
        => await _service.GetForEventAsync(eventId);

    [HttpGet("user/{userId}")]
    public async Task<List<AnnouncementResponse>> GetForUser(int userId)
        => await _service.GetForUserAsync(userId);
}