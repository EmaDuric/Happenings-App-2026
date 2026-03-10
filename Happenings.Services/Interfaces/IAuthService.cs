using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Login(LoginRequest request);

        UserDto Register(UserInsertRequest request);
    }
}


