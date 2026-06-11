using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;

public interface IUserPreferenceService
{
    // Admin � sve preference
    List<UserPreferenceDto> Get();
    // Korisnikove vlastite preference (userId iz JWT-a)
    List<UserPreferenceDto> GetByUser(int userId);

    // Ownership-aware: vracaju null/false kad korisnik nije vlasnik niti admin
    UserPreferenceDto? GetById(int id, int userId, bool isAdmin);
    UserPreferenceDto Insert(UserPreferenceInsertRequest request, int userId);
    UserPreferenceDto? Update(int id, UserPreferenceUpdateRequest request, int userId, bool isAdmin);
    bool Delete(int id, int userId, bool isAdmin);
}
