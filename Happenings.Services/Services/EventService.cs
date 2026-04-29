using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Happenings.Services.Services;

public class EventService
    : BaseCRUDService<Event, EventDto, EventSearchObject, EventInsertRequest, EventUpdateRequest>,
      IEventService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventService(HappeningsContext context, IHttpContextAccessor accessor)
        : base(context)
    {
        _httpContextAccessor = accessor;
    }

    protected override IQueryable<Event> BuildQuery(EventSearchObject search)
    {
        var query = _set
            .Include(e => e.Location)
            .Include(e => e.EventCategory)
            .Include(e=> e.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Name))
            query = query.Where(e => e.Name.Contains(search.Name));

        if (search.EventCategoryId.HasValue)
            query = query.Where(e => e.EventCategoryId == search.EventCategoryId.Value);

        if (!string.IsNullOrWhiteSpace(search.Location))
            query = query.Where(e => e.Location.Name.Contains(search.Location));

        if (search.DateFrom.HasValue)
            query = query.Where(e => e.EventDate >= search.DateFrom.Value);

        if (search.DateTo.HasValue)
            query = query.Where(e => e.EventDate <= search.DateTo.Value);

        return query.OrderBy(e => e.EventDate);
    }

    protected override EventDto MapToDto(Event entity)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            EventDate = entity.EventDate,
            OrganizerId = entity.OrganizerId,
            LocationId = entity.LocationId,
            EventCategoryId = entity.EventCategoryId,
            LocationName = entity.Location?.Name,
            CategoryName = entity.EventCategory?.Name,
            ImageUrl = entity.Images.FirstOrDefault()?.ImageUrl
        };

    protected override Event MapInsertToEntity(EventInsertRequest request)
    {
        // 🔥 USER ID IZ JWT
        var userId = int.Parse(_httpContextAccessor.HttpContext!
            .User
            .FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // 🔥 PRONAĐI ORGANIZER
        var organizer = _context.Organizers
            .FirstOrDefault(x => x.UserId == userId);

        if (organizer == null)
            throw new Exception("User is not an organizer");

        return new Event
        {
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            OrganizerId = organizer.Id, // 🔥 OVDJE JE FIX
            LocationId = request.LocationId,
            EventCategoryId = request.EventCategoryId
        };
    }

    protected override void MapUpdateToEntity(EventUpdateRequest request, Event entity)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.EventDate = request.EventDate;
        entity.LocationId = request.LocationId;
        entity.EventCategoryId = request.EventCategoryId;

        // ❌ NE DIRAJ OrganizerId
    }
}