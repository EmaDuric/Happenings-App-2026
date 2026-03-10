using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Model.Entities;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Happenings.Services.Interfaces;

namespace Happenings.Services.Services;

public class EventTicketTypeService : IEventTicketTypeService
{
    private readonly HappeningsContext _context;

    public EventTicketTypeService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventTicketTypeDto> GetByEvent(int eventId)
    {
        return _context.EventTicketType
            .Where(t => t.EventId == eventId)
            .Select(t => new EventTicketTypeDto
            {
                Id = t.Id,
                EventId = t.EventId,
                Name = t.Name,
                Price = t.Price,
                AvailableQuantity = t.AvailableQuantity
            })
            .ToList();
    }

    public EventTicketTypeDto Insert(EventTicketTypeInsertRequest request)
    {
        var entity = new EventTicketType
        {
            EventId = request.EventId,
            Name = request.Name,
            Price = request.Price,
            AvailableQuantity = request.AvailableQuantity
        };

        _context.EventTicketType.Add(entity);
        _context.SaveChanges();

        return new EventTicketTypeDto
        {
            Id = entity.Id,
            EventId = entity.EventId,
            Name = entity.Name,
            Price = entity.Price,
            AvailableQuantity = entity.AvailableQuantity
        };
    }
}