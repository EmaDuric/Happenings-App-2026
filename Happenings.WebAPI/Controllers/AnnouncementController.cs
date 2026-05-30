using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<AnnouncementResponse> Insert(AnnouncementInsertRequest request)
        => await _service.InsertAsync(request);

    [HttpGet("event/{eventId}")]
    public async Task<List<AnnouncementResponse>> GetForEvent(int eventId)
        => await _service.GetForEventAsync(eventId);

    // Korisnik vidi svoje announcements — userId iz JWT tokena
    [HttpGet("my")]
    public async Task<List<AnnouncementResponse>> GetMy()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return await _service.GetForUserAsync(userId);
    }

    // Admin može dohvatiti announcements za bilo kojeg korisnika
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<List<AnnouncementResponse>> GetForUser(int userId)
        => await _service.GetForUserAsync(userId);
}