using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Interfaces;


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

        // POST: api/Auth/login
        [HttpPost("login")]
        public ActionResult<AuthResponse> Login(LoginRequest request)
        {
            var result = _service.Login(request);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public ActionResult<UserDto> Register(UserInsertRequest request)
        {
            var result = _service.Register(request);

            return Ok(result);
        }
    }
}