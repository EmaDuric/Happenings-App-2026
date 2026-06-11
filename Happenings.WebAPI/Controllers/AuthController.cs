using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Microsoft.AspNetCore.Authorization;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // Greske iz AuthService (UnauthorizedException->401, BusinessRule->400,
        // Conflict->409, NotFound->404) mapira ExceptionMiddleware po tipu.

        // POST: api/Auth/login
        [HttpPost("login")]
        public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
            => Ok(_service.Login(request));

        // POST: api/Auth/register
        [HttpPost("register")]
        public ActionResult<UserDto> Register([FromBody] UserInsertRequest request)
            => Ok(_service.Register(request));

        // POST: api/Auth/forgot-password � generise jednokratni reset token
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var token = _service.ForgotPassword(request);
            // Uvijek isti generic odgovor (ne otkrivamo da li email postoji).
            // Token se za seminar-demo vraca u odgovoru (inace bi isao mailom).
            return Ok(new
            {
                message = "If the email exists, a password reset token has been generated.",
                token
            });
        }

        // POST: api/Auth/reset-password � postavlja novu lozinku uz vazeci token
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _service.ResetPassword(request);
            return Ok(new { message = "Password has been reset successfully" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            _service.ChangePassword(userId, request);
            return Ok(new { message = "Password changed successfully" });
        }
    }
}