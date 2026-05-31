using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IOrganizerRequestService
{
    Task<OrganizerRequestDto> InsertAsync(int userId);
    List<OrganizerRequestDto> GetAll();
    Task ApproveAsync(int id, int adminUserId);
    Task RejectAsync(int id, int adminUserId, string? reason);
}