using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;
public interface IInvitationService
{
    Task<List<InvitationResponse>> GetAsync(int? receiverId);
    Task<InvitationResponse> GetByIdAsync(int id);

    Task<InvitationResponse> InsertAsync(InvitationInsertRequest request);
   

    Task<bool> DeleteAsync(int id);
}