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
            if (entity == null)
                return null;

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

        public ReviewDto Update(int id, ReviewUpdateRequest request)
        {
            var entity = _context.Reviews.Find(id)
                ?? throw new Exception("Review not found");

            entity.Rating = request.Rating;
            entity.Comment = request.Comment;

            _context.SaveChanges();

            return GetById(id)!;
        }

        public void Delete(int id)
        {
            var entity = _context.Reviews.Find(id)
                ?? throw new Exception("Review not found");

            _context.Reviews.Remove(entity);
            _context.SaveChanges();
        }

        public List<EligibleEventDto> GetEligibleEvents(int userId)
        {
            // Eventi za koje user ima ticket (prošli eventi)
            var ticketEventIds = _context.Tickets
                .Include(t => t.Reservation)
                .Where(t => t.Reservation.UserId == userId)
                .Select(t => t.Reservation.EventId)
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
