using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IReportService
{
    List<EventSalesReportDto> GetEventSales();

    List<EventRevenueReportDto> GetRevenuePerEvent();

    List<EventRatingReportDto> GetAverageRatingPerEvent();

    List<EventPopularityDto> GetMostPopularEvents();
}