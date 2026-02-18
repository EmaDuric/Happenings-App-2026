using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Services;

public class EventViewService : IEventViewService
{
    private readonly HappeningsContext _context;

    public EventViewService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventViewDto> GetAll()
    {
        return _context.EventViews
            .Select(e => new EventViewDto
            {
                Id = e.Id,
                ViewedAt = e.ViewedAt,
                UserId = e.UserId,
                EventId = e.EventId
            })
            .ToList();
    }

    public List<EventViewDto> GetByEvent(int eventId)
    {
        return _context.EventViews
            .Where(e => e.EventId == eventId)
            .Select(e => new EventViewDto
            {
                Id = e.Id,
                ViewedAt = e.ViewedAt,
                UserId = e.UserId,
                EventId = e.EventId
            })
            .ToList();
    }

    public List<EventViewDto> GetByUser(int userId)
    {
        return _context.EventViews
            .Where(e => e.UserId == userId)
            .Select(e => new EventViewDto
            {
                Id = e.Id,
                ViewedAt = e.ViewedAt,
                UserId = e.UserId,
                EventId = e.EventId
            })
            .ToList();
    }

    public EventViewDto Insert(EventViewInsertRequest request)
    {
        var entity = new EventView
        {
            UserId = request.UserId,
            EventId = request.EventId,
            ViewedAt = DateTime.UtcNow
        };

        _context.EventViews.Add(entity);
        _context.SaveChanges();

        return new EventViewDto
        {
            Id = entity.Id,
            ViewedAt = entity.ViewedAt,
            UserId = entity.UserId,
            EventId = entity.EventId
        };
    }
}
