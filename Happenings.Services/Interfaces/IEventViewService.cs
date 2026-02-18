using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IEventViewService
{
    List<EventViewDto> GetAll();
    List<EventViewDto> GetByEvent(int eventId);
    List<EventViewDto> GetByUser(int userId);

    EventViewDto Insert(EventViewInsertRequest request);
}
