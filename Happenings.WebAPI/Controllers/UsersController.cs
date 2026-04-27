using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
            => Ok(_service.Get());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
            => Ok(_service.GetById(id));

        [HttpPost]
        public IActionResult Insert(UserInsertRequest request)
            => Ok(_service.Insert(request));

        [HttpPut("{id}")]
        public IActionResult Update(int id, UserUpdateRequest request)
            => Ok(_service.Update(id, request));

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
