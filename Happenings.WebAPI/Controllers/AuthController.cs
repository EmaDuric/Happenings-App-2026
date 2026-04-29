using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

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
        public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = _service.Login(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public ActionResult<UserDto> Register([FromBody] UserInsertRequest request)
        {
            try
            {
                var result = _service.Register(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}