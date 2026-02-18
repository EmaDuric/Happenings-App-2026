using Happenings.Model.Entities;
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
                    EventId = r.EventId
                })
                .ToList();
        }

        public ReservationDto GetById(int id)
        {
            var entity = _context.Reservations.Find(id)
                ?? throw new Exception("Reservation not found");

            return new ReservationDto
            {
                Id = entity.Id,
                ReservedAt = entity.ReservedAt,
                UserId = entity.UserId,
                EventId = entity.EventId
            };
        }

        public ReservationDto Insert(ReservationInsertRequest request)
        {
            var entity = new Reservation
            {
                UserId = request.UserId,
                EventId = request.EventId,
                ReservedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id);
        }

        public ReservationDto Update(int id, ReservationUpdateRequest request)
        {
            var entity = _context.Reservations.Find(id)
                ?? throw new Exception("Reservation not found");

            entity.ReservedAt = request.ReservedAt;

            _context.SaveChanges();
            return GetById(id);
        }

        public void Delete(int id)
        {
            var entity = _context.Reservations.Find(id)
                ?? throw new Exception("Reservation not found");

            _context.Reservations.Remove(entity);
            _context.SaveChanges();
        }
    }
}
