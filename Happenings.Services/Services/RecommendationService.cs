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

        var events = _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Location)
            .Include(e => e.Reservations)
             .Include(e => e.Images)
            .ToList();

        if (!users.Any() || !events.Any())
            return new List<RecommendedEventDto>();

        int userCount = users.Count;
        int eventCount = events.Count;

        var matrix = Matrix<double>.Build.Dense(userCount, eventCount);

        var reservations = _context.Reservations.ToList();
        var eventViews = _context.EventViews.ToList();
        var reviews = _context.Reviews.ToList();

        // =========================
        // MATRICA INTERAKCIJA
        // =========================

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

        foreach (var review in reviews)
        {
            int userIndex = users.FindIndex(u => u.Id == review.UserId);
            int eventIndex = events.FindIndex(e => e.Id == review.EventId);

            if (userIndex >= 0 && eventIndex >= 0)
                matrix[userIndex, eventIndex] = Math.Max(matrix[userIndex, eventIndex], 4);
        }

        int targetUserIndex = users.FindIndex(u => u.Id == userId);

        if (targetUserIndex < 0)
            return new List<RecommendedEventDto>();

        // =========================
        // INTERAKCIJE KORISNIKA
        // =========================

        var interactedEventIds = new HashSet<int>();

        foreach (var r in reservations.Where(x => x.UserId == userId))
            interactedEventIds.Add(r.EventId);

        foreach (var v in eventViews.Where(x => x.UserId == userId))
            interactedEventIds.Add(v.EventId);

        foreach (var r in reviews.Where(x => x.UserId == userId))
            interactedEventIds.Add(r.EventId);

        // =========================
        // COLD START USER
        // =========================

        if (!interactedEventIds.Any())
        {
            return events
                .OrderByDescending(e => e.Reservations.Count)
                .Take(5)
                .Select(e => new RecommendedEventDto
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    EventDate = e.EventDate,
                    EventCategoryId = e.EventCategoryId,
                    CategoryName = e.EventCategory?.Name,
                    LocationName = e.Location?.Name,
                    Score = 0,
                    ImageUrl = e.Images.FirstOrDefault()?.ImageUrl
                })
                .ToList(); // ← .ToList() je DIO Select lanca, ne van njega
        }

        // =========================
        // SVD
        // =========================

        // =========================
        // SVD
        // =========================

        var svd = matrix.Svd(true);

        // ✅ FIX: koristi min(userCount, eventCount) za sigma dimenzije
        int k = Math.Min(userCount, eventCount);

        var sigma = Matrix<double>.Build.Dense(userCount, eventCount);
        for (int i = 0; i < k; i++)
        {
            sigma[i, i] = svd.S[i];
        }

        var reconstructed = svd.U * sigma * svd.VT;

        // =========================
        // GENERISANJE PREPORUKA
        // =========================

        var recommendations = new List<RecommendedEventDto>();

        for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
        {
            var ev = events[eventIndex];

            if (interactedEventIds.Contains(ev.Id))
                continue;

            recommendations.Add(new RecommendedEventDto
            {
                EventId = ev.Id,
                EventName = ev.Name,
                EventDate = ev.EventDate,
                EventCategoryId = ev.EventCategoryId,
                CategoryName = ev.EventCategory?.Name,
                LocationName = ev.Location?.Name,
                Score = reconstructed[targetUserIndex, eventIndex],
                ImageUrl = ev.Images.FirstOrDefault()?.ImageUrl // ← DODAJ
            });
        }

        return recommendations
            .OrderByDescending(x => x.Score)
            .Take(5)
            .ToList();
    }
}