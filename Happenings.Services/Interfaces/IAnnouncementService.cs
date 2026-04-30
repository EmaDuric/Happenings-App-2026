using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> InsertAsync(AnnouncementInsertRequest request);

    Task<List<AnnouncementResponse>> GetForEventAsync(int eventId);

    Task<List<AnnouncementResponse>> GetForUserAsync(int userId);
}