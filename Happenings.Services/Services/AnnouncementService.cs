using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Happenings.Services.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly HappeningsContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AnnouncementService(HappeningsContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AnnouncementResponse> InsertAsync(AnnouncementInsertRequest request)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) throw new Exception("User not authenticated");
        var createdById = int.Parse(userIdClaim);

        var entity = new Announcement
        {
            EventId = request.EventId,
            Title = request.Title,
            Content = request.Content,
            CreatedById = createdById,  // ← iz JWT
            CreatedAt = DateTime.UtcNow
        };

        _context.Announcements.Add(entity);
        await _context.SaveChangesAsync();
        return await Map(entity.Id);
    }

    // ZA ADMIN / ORGANIZER
    public async Task<List<AnnouncementResponse>> GetForEventAsync(int eventId)
    {
        var list = await _context.Announcements
            .Include(e => e.Event)
            .Where(x => x.EventId == eventId)
            .ToListAsync();

        return list.Select(MapSimple).ToList();
    }

    // 🔥 ZA USERA
    public async Task<List<AnnouncementResponse>> GetForUserAsync(int userId)
    {
        var today = DateTime.UtcNow;

        var list = await _context.Announcements
            .Include(a => a.Event)
            .Where(a =>
                a.Event.EventDate > today &&
                _context.Reservations.Any(r =>
                    r.EventId == a.EventId &&
                    r.UserId == userId
                )
            )
            .ToListAsync();

        return list.Select(MapSimple).ToList();
    }

    // MAPPING
    private async Task<AnnouncementResponse> Map(int id)
    {
        var x = await _context.Announcements
            .Include(e => e.Event)
            .FirstOrDefaultAsync(a => a.Id == id);

        return MapSimple(x);
    }

    private AnnouncementResponse MapSimple(Announcement x)
    {
        return new AnnouncementResponse
        {
            Id = x.Id,
            EventId = x.EventId,
            EventName = x.Event.Name,
            Title = x.Title,
            Content = x.Content,
            CreatedAt = x.CreatedAt
        };
    }
}