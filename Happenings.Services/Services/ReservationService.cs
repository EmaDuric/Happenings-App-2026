using Happenings.Model.Entities;
using Happenings.Model.Enums;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services
{
    public class ReservationService : IReservationService
    {
        private readonly HappeningsContext _context;

        public ReservationService(HappeningsContext context)
        {
            _context = context;
        }

        public List<ReservationDto> Get()
        {
            return _context.Reservations
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    ReservedAt = r.ReservedAt,
                    UserId = r.UserId,
                    EventId = r.EventId,
                    EventTicketTypeId = r.EventTicketTypeId,
                    Quantity = r.Quantity,
                    Status = r.Status.ToString()
                })
                .ToList();
        }

        public ReservationDto GetById(int id)
        {
            var entity = _context.Reservations
                .FirstOrDefault(x => x.Id == id)
                ?? throw new Exception("Reservation not found");

            return new ReservationDto
            {
                Id = entity.Id,
                ReservedAt = entity.ReservedAt,
                UserId = entity.UserId,
                EventId = entity.EventId,
                EventTicketTypeId = entity.EventTicketTypeId,
                Quantity = entity.Quantity,
                Status = entity.Status.ToString()
            };
        }

        public ReservationDto Insert(ReservationInsertRequest request)
        {
            var ticketType = _context.EventTicketType
                .FirstOrDefault(x => x.Id == request.EventTicketTypeId);

            if (ticketType == null)
                throw new Exception("Ticket type not found");

            if (ticketType.AvailableQuantity < request.Quantity)
                throw new Exception("Not enough tickets available");

            var entity = new Reservation
            {
                UserId = request.UserId,
                EventId = request.EventId,
                EventTicketTypeId = request.EventTicketTypeId,
                Quantity = request.Quantity,
                ReservedAt = DateTime.UtcNow,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public ReservationDto Update(int id, ReservationUpdateRequest request)
        {
            var entity = _context.Reservations
                .FirstOrDefault(x => x.Id == id)
                ?? throw new Exception("Reservation not found");

            entity.Quantity = request.Quantity;

            _context.SaveChanges();

            return GetById(id);
        }

        public void Delete(int id)
        {
            var entity = _context.Reservations
                .FirstOrDefault(x => x.Id == id)
                ?? throw new Exception("Reservation not found");

            _context.Reservations.Remove(entity);
            _context.SaveChanges();
        }

        public void Approve(int id)
        {
            var reservation = _context.Reservations
                .Include(x => x.EventTicketType)
                .FirstOrDefault(x => x.Id == id);

            if (reservation == null)
                throw new Exception("Reservation not found");

            if (reservation.Status != ReservationStatus.Pending)
                throw new Exception("Reservation already processed");

            if (reservation.EventTicketType.AvailableQuantity < reservation.Quantity)
                throw new Exception("Not enough tickets left");

            reservation.EventTicketType.AvailableQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Approved;

            _context.SaveChanges();
        }

        public void Reject(int id)
        {
            var reservation = _context.Reservations
                .FirstOrDefault(x => x.Id == id);

            if (reservation == null)
                throw new Exception("Reservation not found");

            reservation.Status = ReservationStatus.Rejected;

            _context.SaveChanges();
        }
    }
}