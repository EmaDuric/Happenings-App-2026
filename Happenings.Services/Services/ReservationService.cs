using Happenings.Model.Exceptions;
using Happenings.Model.Entities;
using Happenings.Model.Enums;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Happenings.Services.Services
{
    public class ReservationService : IReservationService
    {
        private readonly HappeningsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReservationService(HappeningsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedException("User not authenticated");
            return int.Parse(claim.Value);
        }

        public List<ReservationDto> Get()
        {
            return _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.EventTicketType)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    ReservedAt = r.ReservedAt,
                    UserId = r.UserId,
                    UserName = _context.Users
                        .Where(u => u.Id == r.UserId)
                        .Select(u => u.Username)
                        .FirstOrDefault(),
                    EventId = r.EventId,
                    EventName = r.Event.Name,
                    EventDate = r.Event.EventDate,
                    EventTicketTypeId = r.EventTicketTypeId,
                    TicketTypeName = r.EventTicketType.Name,
                    Quantity = r.Quantity,
                    Status = r.Status.ToString()
                })
                .ToList();
        }

        public ReservationDto? GetById(int id, int userId, bool isAdmin)
        {
            var entity = _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.EventTicketType)
                .FirstOrDefault(x => x.Id == id);

            if (entity == null) return null;
            if (!isAdmin && entity.UserId != userId) return null;

            return MapToDto(entity);
        }

        // Interno dohvatanje za mapiranje u DTO nakon vec autorizovane akcije
        // (Insert/Update). Bez ownership provjere jer pozivalac vec radi kontekst.
        private ReservationDto GetByIdInternal(int id)
        {
            var entity = _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.EventTicketType)
                .FirstOrDefault(x => x.Id == id)
                ?? throw new NotFoundException("Reservation not found");

            return MapToDto(entity);
        }

        public ReservationDto Insert(ReservationInsertRequest request)
        {
            var userId = GetCurrentUserId();

            if (request.Quantity <= 0)
                throw new BusinessRuleException("Quantity must be greater than zero");

            var ticketType = _context.EventTicketTypes
                .FirstOrDefault(x => x.Id == request.EventTicketTypeId)
                ?? throw new NotFoundException("Ticket type not found");

            if (ticketType.EventId != request.EventId)
                throw new BusinessRuleException("Selected ticket type does not belong to this event");

            if (ticketType.AvailableQuantity < request.Quantity)
                throw new BusinessRuleException("Not enough tickets available");

            var entity = new Reservation
            {
                UserId = userId,
                EventId = request.EventId,
                EventTicketTypeId = request.EventTicketTypeId,
                Quantity = request.Quantity,
                ReservedAt = DateTime.UtcNow,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(entity);
            _context.SaveChanges();

            var dto = GetByIdInternal(entity.Id);
            AddNotification(entity.UserId, "Reservation created",
                $"Your reservation for \"{dto.EventName}\" is pending approval.");
            return dto;
        }

        public ReservationDto? Update(int id, ReservationUpdateRequest request, int userId, bool isAdmin)
        {
            var entity = _context.Reservations.FirstOrDefault(x => x.Id == id);
            if (entity == null) return null;
            if (!isAdmin && entity.UserId != userId) return null;

            if (request.Quantity <= 0)
                throw new BusinessRuleException("Quantity must be greater than zero");

            entity.Quantity = request.Quantity;
            _context.SaveChanges();

            return GetByIdInternal(id);
        }

        public bool Cancel(int id, int userId, bool isAdmin, string? reason)
        {
            var entity = _context.Reservations.FirstOrDefault(x => x.Id == id);
            if (entity == null) return false;
            if (!isAdmin && entity.UserId != userId) return false;

            if (entity.Status == ReservationStatus.Cancelled)
                throw new ConflictException("Reservation already cancelled");

            ChangeStatus(entity, ReservationStatus.Cancelled, userId, reason);

            _context.SaveChanges();

            var dto = GetByIdInternal(id);
            AddNotification(dto.UserId, "Reservation cancelled",
                $"Your reservation for \"{dto.EventName}\" was cancelled. Reason: {entity.CancellationReason}");
            return true;
        }

        // Admin approve � delegira na centralizovani tok
        public void Approve(int id)
        {
            var adminId = GetCurrentUserId();
            ApproveReservation(id, adminId);

            // Notifikacija za admin-approve. Payment-driven approve NE notificira ovdje
            // jer Worker vec salje payment-success notifikaciju (izbjegava duple).
            var dto = GetByIdInternal(id);
            AddNotification(dto.UserId, "Reservation approved",
                $"Your reservation for \"{dto.EventName}\" has been approved.");
        }

        // Jedinstveno mjesto za odobrenje rezervacije: provjeri da je jos Pending i
        // da ima dovoljno dostupne kolicine (osvjezeno iz baze), dekrementiraj stok,
        // postavi Approved + audit. Koriste ga i admin approve i payment tokovi, pa
        // dekrement/odobrenje vise nije dupliran u dvije odvojene implementacije.
        public void ApproveReservation(int reservationId, int approvedByUserId)
        {
            var reservation = _context.Reservations
                .Include(x => x.EventTicketType)
                .FirstOrDefault(x => x.Id == reservationId)
                ?? throw new NotFoundException("Reservation not found");

            // Osvjezi iz baze � bitno kad se zove iz payment toka nakon provider
            // poziva (zatvara race izmedju kreiranja intenta/ordera i capture/confirm).
            _context.Entry(reservation).Reload();
            if (reservation.EventTicketType != null)
                _context.Entry(reservation.EventTicketType).Reload();

            if (reservation.Status != ReservationStatus.Pending)
                throw new ConflictException($"Reservation cannot be approved. Current status: {reservation.Status}");

            if (reservation.EventTicketType == null)
                throw new NotFoundException("Ticket type not found");

            if (reservation.EventTicketType.AvailableQuantity < reservation.Quantity)
                throw new BusinessRuleException("Not enough tickets left");

            reservation.EventTicketType.AvailableQuantity -= reservation.Quantity;
            ChangeStatus(reservation, ReservationStatus.Approved, approvedByUserId, null);

            _context.SaveChanges();
        }

        public void Reject(int id, string? reason)
        {
            var adminId = GetCurrentUserId();

            var reservation = _context.Reservations
                .FirstOrDefault(x => x.Id == id)
                ?? throw new NotFoundException("Reservation not found");

            if (reservation.Status != ReservationStatus.Pending)
                throw new ConflictException("Reservation already processed");

            ChangeStatus(reservation, ReservationStatus.Rejected, adminId, reason);

            _context.SaveChanges();

            var dto = GetByIdInternal(id);
            AddNotification(dto.UserId, "Reservation rejected",
                $"Your reservation for \"{dto.EventName}\" was rejected. Reason: {reservation.RejectedReason}");
        }

        // Sistemska notifikacija za bitne promjene rezervacije (create/approve/
        // reject/cancel). Payment-success notifikaciju i dalje salje Worker.
        private void AddNotification(int userId, string title, string message)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
            _context.SaveChanges();
        }

        public void Complete(int id)
        {
            var adminId = GetCurrentUserId();

            var reservation = _context.Reservations
                .FirstOrDefault(x => x.Id == id)
                ?? throw new NotFoundException("Reservation not found");

            // Zavrsiti se moze samo odobrena rezervacija (npr. nakon odrzanog eventa).
            if (reservation.Status != ReservationStatus.Approved)
                throw new ConflictException("Only approved reservations can be completed");

            ChangeStatus(reservation, ReservationStatus.Completed, adminId, null);

            _context.SaveChanges();

            var dto = GetByIdInternal(id);
            AddNotification(dto.UserId, "Reservation completed",
                $"Your reservation for \"{dto.EventName}\" is marked as completed.");
        }

        // Centralizovana promjena statusa + upis audit traga (vrijeme, ko je radio
        // akciju i stvarni razlog kod reject/cancel). Jedno mjesto za sve tranzicije.
        private static void ChangeStatus(Reservation r, ReservationStatus status, int byUserId, string? reason)
        {
            var now = DateTime.UtcNow;
            r.Status = status;

            switch (status)
            {
                case ReservationStatus.Approved:
                    r.ApprovedAt = now;
                    r.ApprovedByUserId = byUserId;
                    break;
                case ReservationStatus.Rejected:
                    r.RejectedAt = now;
                    r.RejectedByUserId = byUserId;
                    r.RejectedReason = string.IsNullOrWhiteSpace(reason) ? "Rejected by admin" : reason.Trim();
                    break;
                case ReservationStatus.Cancelled:
                    r.CancelledAt = now;
                    r.CancelledByUserId = byUserId;
                    r.CancellationReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled by user" : reason.Trim();
                    break;
                case ReservationStatus.Completed:
                    r.CompletedAt = now;
                    r.CompletedByUserId = byUserId;
                    break;
            }
        }

        public List<ReservationDto> GetByUserId(int userId)
        {
            return _context.Reservations
                .Include(r => r.Event)
                .Include(r => r.EventTicketType)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    EventId = r.EventId,
                    EventName = r.Event.Name,
                    EventDate = r.Event.EventDate,
                    EventTicketTypeId = r.EventTicketTypeId,
                    TicketTypeName = r.EventTicketType.Name,
                    Quantity = r.Quantity,
                    Status = r.Status.ToString(),
                    ReservedAt = r.ReservedAt,
                    UserId = r.UserId
                })
                .ToList();
        }

        private ReservationDto MapToDto(Reservation r) => new ReservationDto
        {
            Id = r.Id,
            ReservedAt = r.ReservedAt,
            UserId = r.UserId,
            EventId = r.EventId,
            EventName = r.Event?.Name,
            EventDate = r.Event?.EventDate ?? DateTime.MinValue,
            EventTicketTypeId = r.EventTicketTypeId,
            TicketTypeName = r.EventTicketType?.Name,
            Quantity = r.Quantity,
            Status = r.Status.ToString()
        };
    }
}