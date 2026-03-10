using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Happenings.Model.Enums;

public class ReportService : IReportService
{
    private readonly HappeningsContext _context;

    public ReportService(HappeningsContext context)
    {
        _context = context;
    }

    public List<EventSalesReportDto> GetEventSales()
    {
        return _context.Reservations
            .Where(r => r.Status == ReservationStatus.Approved)
            .GroupBy(r => new { r.EventId, r.Event.Name })
            .Select(g => new EventSalesReportDto
            {
                EventId = g.Key.EventId,
                EventName = g.Key.Name,
                TicketsSold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TicketsSold)
            .ToList();
    }

    public List<EventRevenueReportDto> GetRevenuePerEvent()
    {
        return _context.Reservations
            .Where(r => r.Status == ReservationStatus.Approved)
            .Include(r => r.EventTicketType)
            .GroupBy(r => new { r.EventId, r.Event.Name })
            .Select(g => new EventRevenueReportDto
            {
                EventId = g.Key.EventId,
                EventName = g.Key.Name,
                Revenue = g.Sum(x => x.Quantity * x.EventTicketType.Price)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    public List<EventRatingReportDto> GetAverageRatingPerEvent()
    {
        return _context.Reviews
            .GroupBy(r => new { r.EventId, r.Event.Name })
            .Select(g => new EventRatingReportDto
            {
                EventId = g.Key.EventId,
                EventName = g.Key.Name,
                AverageRating = g.Average(x => x.Rating)
            })
            .OrderByDescending(x => x.AverageRating)
            .ToList();
    }
}