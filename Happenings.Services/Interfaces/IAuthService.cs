using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Login(LoginRequest request);
        UserDto Register(UserInsertRequest request);
        void ChangePassword(int userId, ChangePasswordRequest request);

        // Generise jednokratni reset token (vraca ga radi seminar-demo � u realnoj
        // aplikaciji bi se slao mailom). Vraca null ako korisnik ne postoji.
        string? ForgotPassword(ForgotPasswordRequest request);
        void ResetPassword(ResetPasswordRequest request);
    }
}


