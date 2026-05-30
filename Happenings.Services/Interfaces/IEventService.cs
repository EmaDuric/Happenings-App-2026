using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Model.Search;

namespace Happenings.Services.Interfaces;

public interface IEventService : ICRUDService<EventDto, EventSearchObject, EventInsertRequest, EventUpdateRequest>
{
    Task<bool> DeleteAsync(int id);
    Task<EventDto?> UpdateAsync(int id, EventUpdateRequest request, int userId, bool isAdmin);
    Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
}