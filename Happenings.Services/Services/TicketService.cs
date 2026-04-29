using Happenings.Model.Entities;
using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public class TicketService : ITicketService
{
    private readonly HappeningsContext _context;
    private readonly QrCodeService _qrService;

    public TicketService(HappeningsContext context, QrCodeService qrService)
    {
        _context = context;
        _qrService = qrService;
    }

    public List<TicketDto> Get()
    {
        return _context.Tickets
            .Select(x => new TicketDto
            {
                Id = x.Id,
                ReservationId = x.ReservationId,
                QRCode = x.QRCode,
                IsUsed = x.IsUsed,
                GeneratedAt = x.GeneratedAt
            }).ToList();
    }

    public TicketDto? GetById(int id)
    {
        var entity = _context.Tickets.Find(id);

        if (entity == null)
            return null;

        return new TicketDto
        {
            Id = entity.Id,
            ReservationId = entity.ReservationId,
            QRCode = entity.QRCode,
            IsUsed = entity.IsUsed,
            GeneratedAt = entity.GeneratedAt
        };
    }

    public TicketDto Insert(TicketInsertRequest request)
    {
        var entity = new Ticket
        {
            ReservationId = request.ReservationId,
            IsUsed = false
        };

        _context.Tickets.Add(entity);
        _context.SaveChanges();

        // nakon SaveChanges imamo ID
        entity.QRCode = _qrService.GenerateQRCode(entity.Id.ToString());

        _context.SaveChanges();

        return new TicketDto
        {
            Id = entity.Id,
            ReservationId = entity.ReservationId,
            QRCode = entity.QRCode,
            IsUsed = entity.IsUsed,
            GeneratedAt = entity.GeneratedAt
        };
    }

    public List<TicketDto> GetByUserId(int userId)
    {
        return _context.Tickets
            .Include(t => t.Reservation)
            .ThenInclude(r => r.Event)
            .Where(t => t.Reservation.UserId == userId)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                ReservationId = t.ReservationId,
                QRCode = t.QRCode,
                IsUsed = t.IsUsed,
                GeneratedAt = t.GeneratedAt,

                // 🔥 DODAJ OVO
                EventName = t.Reservation.Event.Name,
                EventDate = t.Reservation.Event.EventDate,
                Location = t.Reservation.Event.Location.Name
            })
            .ToList();
    }
}