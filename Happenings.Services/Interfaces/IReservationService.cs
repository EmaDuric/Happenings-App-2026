using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IReservationService
    {
        List<ReservationDto> Get();
        ReservationDto? GetById(int id, int userId, bool isAdmin);
        ReservationDto Insert(ReservationInsertRequest request);
        ReservationDto? Update(int id, ReservationUpdateRequest request, int userId, bool isAdmin);
        bool Cancel(int id, int userId, bool isAdmin);
        void Approve(int id);
        void Reject(int id);
        List<ReservationDto> GetByUserId(int userId);
    }
}