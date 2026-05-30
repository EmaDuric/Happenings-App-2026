using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        // ADMIN ONLY — lista svih korisnika
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
            => Ok(_service.Get());

        // ADMIN ONLY — dohvat korisnika po ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
            => Ok(_service.GetById(id));

        // MY PROFILE — korisnik vidi svoje podatke iz JWT tokena
        [HttpGet("my")]
        public IActionResult GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var user = _service.GetById(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // MY PROFILE UPDATE — korisnik mijenja samo svoje podatke
        [HttpPut("my")]
        public IActionResult UpdateMyProfile([FromBody] UserUpdateRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            return Ok(_service.Update(userId, request));
        }

        // ADMIN ONLY — kreiranje korisnika
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Insert(UserInsertRequest request)
            => Ok(_service.Insert(request));

        // ADMIN ONLY — update bilo kojeg korisnika
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, UserUpdateRequest request)
            => Ok(_service.Update(id, request));

        // ADMIN ONLY — brisanje korisnika
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}