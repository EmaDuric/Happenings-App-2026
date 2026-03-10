using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IRecommendationService
{
    List<RecommendedEventDto> GetRecommendedEvents(int userId);
}