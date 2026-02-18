using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;

public interface IUserPreferenceService
{
    List<UserPreferenceDto> Get();
    UserPreferenceDto? GetById(int id);
    UserPreferenceDto Insert(UserPreferenceInsertRequest request);
    UserPreferenceDto? Update(int id, UserPreferenceUpdateRequest request);
    bool Delete(int id);
}
