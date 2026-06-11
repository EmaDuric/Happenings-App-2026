using Happenings.Model.Exceptions;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly HappeningsContext _context;

        public ReviewService(HappeningsContext context)
        {
            _context = context;
        }

        public List<ReviewDto> GetAll()
        {
            return _context.Reviews
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    UserId = r.UserId,
                    EventId = r.EventId
                })
                .ToList();
        }

        public ReviewDto? GetById(int id)
        {
            var entity = _context.Reviews.Find(id);
            if (entity == null) return null;

            return new ReviewDto
            {
                Id = entity.Id,
                Rating = entity.Rating,
                Comment = entity.Comment,
                UserId = entity.UserId,
                EventId = entity.EventId
            };
        }

        public ReviewDto Insert(ReviewInsertRequest request)
        {
            // Validacija ratinga 1-5
            if (request.Rating < 1 || request.Rating > 5)
                throw new BusinessRuleException("Rating must be between 1 and 5");

            // Provjeri da korisnik ima ticket za taj event
            var hasTicket = _context.Tickets
                .Any(t => t.UserId == request.UserId && t.EventId == request.EventId);

            if (!hasTicket)
                throw new BusinessRuleException("You can only review events you have attended");

            // Provjeri da event je pro�ao
            var ev = _context.Events.Find(request.EventId);
            if (ev != null && ev.EventDate > DateTime.UtcNow)
                throw new BusinessRuleException("You can only review past events");

            // Provjeri duplikat � samo jedna recenzija po korisniku po eventu
            var existing = _context.Reviews
                .FirstOrDefault(r => r.UserId == request.UserId && r.EventId == request.EventId);

            if (existing != null)
                throw new ConflictException("You have already reviewed this event");

            var entity = new Review
            {
                Rating = request.Rating,
                Comment = request.Comment,
                UserId = request.UserId,
                EventId = request.EventId
            };

            _context.Reviews.Add(entity);
            _context.SaveChanges();

            return GetById(entity.Id)!;
        }

        // Ownership provjera za Update
        public ReviewDto? Update(int id, ReviewUpdateRequest request, int userId, bool isAdmin)
        {
            var entity = _context.Reviews.Find(id);
            if (entity == null) return null;
            if (!isAdmin && entity.UserId != userId) return null; // Forbidden

            if (request.Rating < 1 || request.Rating > 5)
                throw new BusinessRuleException("Rating must be between 1 and 5");

            entity.Rating = request.Rating;
            entity.Comment = request.Comment;
            _context.SaveChanges();

            return GetById(id);
        }

        // Ownership provjera za Delete
        public bool Delete(int id, int userId, bool isAdmin)
        {
            var entity = _context.Reviews.Find(id);
            if (entity == null) return false;
            if (!isAdmin && entity.UserId != userId) return false; // Forbidden

            _context.Reviews.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public List<EligibleEventDto> GetEligibleEvents(int userId)
        {
            var ticketEventIds = _context.Tickets
                .Where(t => t.UserId == userId)
                .Select(t => t.EventId)
                .Distinct()
                .ToList();

            var events = _context.Events
                .Include(e => e.Images)
                .Where(e => ticketEventIds.Contains(e.Id))
                .ToList();

            var myReviews = _context.Reviews
                .Where(r => r.UserId == userId)
                .ToList();

            return events.Select(e =>
            {
                var review = myReviews.FirstOrDefault(r => r.EventId == e.Id);
                return new EligibleEventDto
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    EventDate = e.EventDate,
                    ImageUrl = e.Images.FirstOrDefault()?.ImageUrl,
                    ExistingReviewId = review?.Id,
                    ExistingRating = review?.Rating,
                    ExistingComment = review?.Comment
                };
            }).ToList();
        }

        public List<ReviewDto> GetMyReviewedEvents(int userId)
        {
            return _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    EventId = r.EventId
                })
                .ToList();
        }
    }
}