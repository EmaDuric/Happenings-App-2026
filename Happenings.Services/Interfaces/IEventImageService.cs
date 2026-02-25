using Happenings.Model.Responses;
using Happenings.Model.Requests;

public interface IEventImageService
{
    List<EventImageDto> GetAll();
    EventImageDto? GetById(int id);
    List<EventImageDto> GetByEvent(int eventId);
    EventImageDto Insert(EventImageInsertRequest request);
    EventImageDto Update(int id, EventImageUpdateRequest request);
    bool Delete(int id);
}
