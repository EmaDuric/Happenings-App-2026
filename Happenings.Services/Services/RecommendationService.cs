using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public class RecommendationService : IRecommendationService
{
    private readonly HappeningsContext _context;

    public RecommendationService(HappeningsContext context)
    {
        _context = context;
    }

    public List<RecommendedEventDto> GetRecommendedEvents(int userId)
    {
        var users = _context.Users.ToList();
        var now = DateTime.UtcNow;

        // Filtriramo samo buduće evente
        var events = _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Location)
            .Include(e => e.Reservations)
            .Include(e => e.Images)
            .Where(e => e.EventDate > now)
            .ToList();

        if (!users.Any() || !events.Any())
            return new List<RecommendedEventDto>();

        int userCount = users.Count;
        int eventCount = events.Count;

        var matrix = Matrix<double>.Build.Dense(userCount, eventCount);

        var reservations = _context.Reservations.ToList();
        var eventViews = _context.EventViews.ToList();
        var reviews = _context.Reviews.ToList();

        // MATRICA INTERAKCIJA
        foreach (var reservation in reservations)
        {
            int userIndex = users.FindIndex(u => u.Id == reservation.UserId);
            int eventIndex = events.FindIndex(e => e.Id == reservation.EventId);
            if (userIndex >= 0 && eventIndex >= 0)
                matrix[userIndex, eventIndex] = Math.Max(matrix[userIndex, eventIndex], 5);
        }

        foreach (var view in eventViews)
        {
            int userIndex = users.FindIndex(u => u.Id == view.UserId);
            int eventIndex = events.FindIndex(e => e.Id == view.EventId);
            if (userIndex >= 0 && eventIndex >= 0)
                matrix[userIndex, eventIndex] = Math.Max(matrix[userIndex, eventIndex], 3);
        }

        // Koristi stvarni Rating iz recenzije umjesto fiksne vrijednosti 4
        foreach (var review in reviews)
        {
            int userIndex = users.FindIndex(u => u.Id == review.UserId);
            int eventIndex = events.FindIndex(e => e.Id == review.EventId);
            if (userIndex >= 0 && eventIndex >= 0)
            {
                // Normalizuj rating na skalu 1-5 → za matricu koristimo stvarni rating
                double ratingScore = review.Rating; // 1-5
                matrix[userIndex, eventIndex] = Math.Max(matrix[userIndex, eventIndex], ratingScore);
            }
        }

        int targetUserIndex = users.FindIndex(u => u.Id == userId);
        if (targetUserIndex < 0)
            return new List<RecommendedEventDto>();

        // INTERAKCIJE KORISNIKA
        var interactedEventIds = new HashSet<int>();
        foreach (var r in reservations.Where(x => x.UserId == userId))
            interactedEventIds.Add(r.EventId);
        foreach (var v in eventViews.Where(x => x.UserId == userId))
            interactedEventIds.Add(v.EventId);
        foreach (var r in reviews.Where(x => x.UserId == userId))
            interactedEventIds.Add(r.EventId);

        // COLD START — popularity-based
        if (!interactedEventIds.Any())
        {
            return events
                .OrderByDescending(e => e.Reservations.Count)
                .Take(6)
                .Select(e => new RecommendedEventDto
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    EventDate = e.EventDate,
                    EventCategoryId = e.EventCategoryId,
                    CategoryName = e.EventCategory?.Name,
                    LocationName = e.Location?.Name,
                    Score = 0,
                    Reason = "Popular event",
                    ImageUrl = e.Images.FirstOrDefault()?.ImageUrl
                })
                .ToList();
        }

        // TRUNCATED SVD — zadrzavamo samo top-k latentnih faktora (k < rang) da
        // model generalizuje i predvidi i ne-vidjene evente. Puni rang bi samo
        // reprodukovao ulaznu matricu (score ~0 za sve ne-interagovane evente).
        var svd = matrix.Svd(true);
        int rank = Math.Min(userCount, eventCount);
        int k = Math.Max(1, Math.Min(rank - 1, 10)); // do 10 latentnih faktora

        var sigma = Matrix<double>.Build.Dense(userCount, eventCount);
        for (int i = 0; i < k; i++)
            sigma[i, i] = svd.S[i];

        var reconstructed = svd.U * sigma * svd.VT;

        // GENERISANJE PREPORUKA
        var recommendations = new List<RecommendedEventDto>();

        for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
        {
            var ev = events[eventIndex];

            if (interactedEventIds.Contains(ev.Id))
                continue;

            var score = reconstructed[targetUserIndex, eventIndex];

            // Generiši kratko objašnjenje preporuke
            string reason = GenerateReason(ev, score, reservations, reviews);

            recommendations.Add(new RecommendedEventDto
            {
                EventId = ev.Id,
                EventName = ev.Name,
                EventDate = ev.EventDate,
                EventCategoryId = ev.EventCategoryId,
                CategoryName = ev.EventCategory?.Name,
                LocationName = ev.Location?.Name,
                Score = score,
                Reason = reason,
                ImageUrl = ev.Images.FirstOrDefault()?.ImageUrl
            });
        }

        return recommendations
            .OrderByDescending(x => x.Score)
            .Take(6)
            .ToList();
    }

    private string GenerateReason(
        Happenings.Model.Entities.Event ev,
        double score,
        List<Happenings.Model.Entities.Reservation> reservations,
        List<Happenings.Model.Entities.Review> reviews)
    {
        var reservationCount = reservations.Count(r => r.EventId == ev.Id);
        var avgRating = reviews.Where(r => r.EventId == ev.Id).Any()
            ? reviews.Where(r => r.EventId == ev.Id).Average(r => r.Rating)
            : 0;

        if (avgRating >= 4)
            return $"Highly rated ({avgRating:F1}★)";
        if (reservationCount > 10)
            return $"Popular — {reservationCount} reservations";
        if (score > 3)
            return "Based on your interests";
        return "Recommended for you";
    }
}