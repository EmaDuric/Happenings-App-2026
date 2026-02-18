using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IUserService
    {
        List<UserDto> Get();
        UserDto GetById(int id);
        UserDto Insert(UserInsertRequest request);
        UserDto Update(int id, UserUpdateRequest request);
        void Delete(int id);
    }
}
