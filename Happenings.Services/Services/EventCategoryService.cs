using Happenings.Model.DTOs;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;

namespace Happenings.Services.Services;

public class EventCategoryService : IEventCategoryService
{
    private readonly HappeningsContext _context;

    public EventCategoryService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventCategoryDto> Get()
    {
        return _context.EventCategories
            .Select(x => new EventCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToList();
    }

    public EventCategoryDto? GetById(int id)
    {
        var entity = _context.EventCategories.Find(id);
        if (entity == null) return null;

        return new EventCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public EventCategoryDto Insert(EventCategoryInsertRequest request)
    {
        var entity = new EventCategory
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.EventCategories.Add(entity);
        _context.SaveChanges();

        return new EventCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public EventCategoryDto Update(int id, EventCategoryUpdateRequest request)
    {
        var entity = _context.EventCategories.Find(id);
        if (entity == null)
            throw new Exception("EventCategory not found");

        entity.Name = request.Name;
        entity.Description = request.Description;

        _context.SaveChanges();

        return new EventCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public bool Delete(int id)
    {
        var entity = _context.EventCategories.Find(id);

        if (entity == null)
            return false;

        var hasEvents = _context.Events.Any(e => e.EventCategoryId == id);

        if (hasEvents)
            throw new Exception("Category cannot be deleted because events exist.");

        _context.EventCategories.Remove(entity);
        _context.SaveChanges();

        return true;
    }
}
