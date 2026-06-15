using Happenings.Model.Exceptions;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class EventImageService : IEventImageService
{
    private readonly HappeningsContext _context;

    public EventImageService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventImageDto> GetAll()
    {
        return _context.EventImages.AsEnumerable().Select(MapToDto).ToList();
    }

    public EventImageDto? GetById(int id)
    {
        var entity = _context.EventImages.Find(id);
        return entity == null ? null : MapToDto(entity);
    }

    public List<EventImageDto> GetByEvent(int eventId)
    {
        return _context.EventImages
            .Where(i => i.EventId == eventId)
            .AsEnumerable()
            .Select(MapToDto)
            .ToList();
    }

    public EventImageDto? Insert(EventImageInsertRequest request, int userId, bool isAdmin)
    {
        if (!CanManageEvent(request.EventId, userId, isAdmin))
            return null;

        var entity = new EventImage
        {
            ImageUrl = request.ImageUrl,
            EventId = request.EventId
        };

        _context.EventImages.Add(entity);
        _context.SaveChanges();

        return MapToDto(entity);
    }

    public EventImageDto? Update(int id, EventImageUpdateRequest request, int userId, bool isAdmin)
    {
        var entity = _context.EventImages.Find(id)
            ?? throw new NotFoundException("EventImage not found");

        if (!CanManageEvent(entity.EventId, userId, isAdmin))
            return null;

        entity.ImageUrl = request.ImageUrl;
        _context.SaveChanges();

        return MapToDto(entity);
    }

    public bool Delete(int id, int userId, bool isAdmin)
    {
        var entity = _context.EventImages.Find(id)
            ?? throw new NotFoundException("EventImage not found");

        if (!CanManageEvent(entity.EventId, userId, isAdmin))
            return false;

        _context.EventImages.Remove(entity);
        _context.SaveChanges();
        return true;
    }

    // Admin moze sve; organizator samo slike vlastitih eventa.
    private bool CanManageEvent(int eventId, int userId, bool isAdmin)
    {
        var ev = _context.Events.Find(eventId)
            ?? throw new NotFoundException("Event not found");

        if (isAdmin) return true;

        var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
        return organizer != null && ev.OrganizerId == organizer.Id;
    }

    private static EventImageDto MapToDto(EventImage i) => new()
    {
        Id = i.Id,
        ImageUrl = i.ImageUrl,
        EventId = i.EventId
    };
}
