using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces
{
    public interface IOrganizerService
    {
        List<OrganizerDto> GetAll();
        OrganizerDto GetById(int id);
        OrganizerDto Insert(OrganizerInsertRequest request);
        OrganizerDto Update(int id, OrganizerUpdateRequest request);
        void Delete(int id);
    }
}
