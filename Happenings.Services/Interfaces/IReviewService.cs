using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IReviewService
    {
        List<ReviewDto> GetAll();
        ReviewDto? GetById(int id);

        ReviewDto Insert(ReviewInsertRequest request);
        ReviewDto? Update(int id, ReviewUpdateRequest request, int userId, bool isAdmin);
        bool Delete(int id, int userId, bool isAdmin);
        List<EligibleEventDto> GetEligibleEvents(int userId);
        List<ReviewDto> GetMyReviewedEvents(int userId);
    }
}
