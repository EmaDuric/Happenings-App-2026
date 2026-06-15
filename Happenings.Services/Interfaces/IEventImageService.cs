using Happenings.Model.Responses;
using Happenings.Model.Requests;

public interface IEventImageService
{
    List<EventImageDto> GetAll();
    EventImageDto? GetById(int id);
    List<EventImageDto> GetByEvent(int eventId);

    // Ownership-aware: null (Insert/Update) ili false (Delete) kad korisnik nije
    // admin niti organizator eventa kojem slika pripada.
    EventImageDto? Insert(EventImageInsertRequest request, int userId, bool isAdmin);
    EventImageDto? Update(int id, EventImageUpdateRequest request, int userId, bool isAdmin);
    bool Delete(int id, int userId, bool isAdmin);
}
