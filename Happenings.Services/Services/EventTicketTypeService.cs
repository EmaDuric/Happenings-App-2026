using Happenings.Model.Exceptions;
﻿using Happenings.Model.Requests;
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
            .AsEnumerable()
            .Select(MapToDto)
            .ToList();
    }

    public EventTicketTypeDto? GetById(int id)
    {
        var t = _context.EventTicketTypes.Find(id);
        return t == null ? null : MapToDto(t);
    }

    public EventTicketTypeDto? Insert(EventTicketTypeInsertRequest request, int userId, bool isAdmin)
    {
        // Ne dozvoljavamo dodavanje tipa ulaznice na tudi event.
        if (!CanManageEvent(request.EventId, userId, isAdmin))
            return null;

        var entity = new EventTicketType
        {
            EventId = request.EventId,
            Name = request.Name,
            Price = request.Price,
            AvailableQuantity = request.AvailableQuantity
        };

        _context.EventTicketTypes.Add(entity);
        _context.SaveChanges();

        return MapToDto(entity);
    }

    public EventTicketTypeDto? Update(int id, EventTicketTypeInsertRequest request, int userId, bool isAdmin)
    {
        var entity = _context.EventTicketTypes.Find(id)
            ?? throw new NotFoundException("Ticket type not found");

        if (!CanManageEvent(entity.EventId, userId, isAdmin))
            return null;

        entity.Name = request.Name;
        entity.Price = request.Price;
        entity.AvailableQuantity = request.AvailableQuantity;

        _context.SaveChanges();
        return MapToDto(entity);
    }

    public bool Delete(int id, int userId, bool isAdmin)
    {
        var entity = _context.EventTicketTypes.Find(id)
            ?? throw new NotFoundException("Ticket type not found");

        if (!CanManageEvent(entity.EventId, userId, isAdmin))
            return false;

        _context.EventTicketTypes.Remove(entity);
        _context.SaveChanges();
        return true;
    }

    // Admin moze sve; organizator samo tipove ulaznica vlastitih eventa.
    private bool CanManageEvent(int eventId, int userId, bool isAdmin)
    {
        var ev = _context.Events.Find(eventId)
            ?? throw new NotFoundException("Event not found");

        if (isAdmin) return true;

        var organizer = _context.Organizers.FirstOrDefault(o => o.UserId == userId);
        return organizer != null && ev.OrganizerId == organizer.Id;
    }

    private static EventTicketTypeDto MapToDto(EventTicketType t) => new()
    {
        Id = t.Id,
        EventId = t.EventId,
        Name = t.Name,
        Price = t.Price,
        AvailableQuantity = t.AvailableQuantity
    };
}