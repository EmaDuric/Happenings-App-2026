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
        return _context.EventImages
            .Select(i => new EventImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                EventId = i.EventId
            }).ToList();
    }

    public EventImageDto? GetById(int id)
    {
        var entity = _context.EventImages.Find(id);
        if (entity == null) return null;

        return new EventImageDto
        {
            Id = entity.Id,
            ImageUrl = entity.ImageUrl,
            EventId = entity.EventId
        };
    }

    public List<EventImageDto> GetByEvent(int eventId)
    {
        return _context.EventImages
            .Where(i => i.EventId == eventId)
            .Select(i => new EventImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                EventId = i.EventId
            }).ToList();
    }

    public EventImageDto Insert(EventImageInsertRequest request)
    {
        var entity = new EventImage
        {
            ImageUrl = request.ImageUrl,
            EventId = request.EventId
        };

        _context.EventImages.Add(entity);
        _context.SaveChanges();

        return new EventImageDto
        {
            Id = entity.Id,
            ImageUrl = entity.ImageUrl,
            EventId = entity.EventId
        };
    }

    public EventImageDto Update(int id, EventImageUpdateRequest request)
    {
        var entity = _context.EventImages.Find(id);
        if (entity == null)
            throw new Exception("EventImage not found");

        entity.ImageUrl = request.ImageUrl;

        _context.SaveChanges();

        return new EventImageDto
        {
            Id = entity.Id,
            ImageUrl = entity.ImageUrl,
            EventId = entity.EventId
        };
    }

    public bool Delete(int id)
    {
        var entity = _context.EventImages.Find(id);
        if (entity == null) return false;

        _context.EventImages.Remove(entity);
        _context.SaveChanges();
        return true;
    }
}
