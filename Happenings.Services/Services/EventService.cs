using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public class EventService
    : BaseCRUDService<Event, EventDto, EventSearchObject, EventInsertRequest, EventUpdateRequest>,
      IEventService
{
    public EventService(HappeningsContext context) : base(context) { }

    protected override IQueryable<Event> BuildQuery(EventSearchObject search)
    {
        var query = _set.AsQueryable();

        // opcionalno: include ako ti treba
        // query = query.Include(e => e.Location).Include(e => e.Organizer);

        if (!string.IsNullOrWhiteSpace(search.Name))
            query = query.Where(e => e.Name.Contains(search.Name));

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
            LocationId = entity.LocationId
        };

    protected override Event MapInsertToEntity(EventInsertRequest request)
        => new()
        {
            Name = request.Name,
            Description = request.Description,
            EventDate = request.EventDate,
            OrganizerId = request.OrganizerId,
            LocationId = request.LocationId
        };

    protected override void MapUpdateToEntity(EventUpdateRequest request, Event entity)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.EventDate = request.EventDate;
        entity.OrganizerId = request.OrganizerId;
        entity.LocationId = request.LocationId;
    }
}
