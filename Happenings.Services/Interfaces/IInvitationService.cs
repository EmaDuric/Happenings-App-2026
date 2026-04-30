using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface IInvitationService
{
    Task<List<InvitationResponse>> GetAsync(int? receiverId);

    Task<InvitationResponse> GetByIdAsync(int id);

    Task<InvitationResponse> InsertAsync(InvitationInsertRequest request);

    Task UpdateStatusAsync(int id, string status);

    Task<bool> DeleteAsync(int id);
}