using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IReservationService
    {
        List<ReservationDto> Get();
        ReservationDto GetById(int id);
        ReservationDto Insert(ReservationInsertRequest request);
        ReservationDto Update(int id, ReservationUpdateRequest request);
        void Delete(int id);
        void Approve(int id);
        void Reject(int id);
        List<ReservationDto> GetByUserId(int userId);

    }
}
