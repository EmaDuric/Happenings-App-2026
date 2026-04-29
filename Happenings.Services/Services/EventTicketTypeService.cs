using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Entities;
using Happenings.Services.Database;
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
        return _context.EventTicketTypes
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

        _context.EventTicketTypes.Add(entity);
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
    public object Update(int id, EventTicketTypeInsertRequest request)
    {
        var entity = _context.EventTicketTypes.Find(id)
            ?? throw new Exception("Ticket type not found");

        entity.Name = request.Name;
        entity.Price = request.Price;
        entity.AvailableQuantity = request.AvailableQuantity;

        _context.SaveChanges();
        return entity;
    }

    public void Delete(int id)
    {
        var entity = _context.EventTicketTypes.Find(id)
            ?? throw new Exception("Ticket type not found");

        _context.EventTicketTypes.Remove(entity);
        _context.SaveChanges();
    }
}