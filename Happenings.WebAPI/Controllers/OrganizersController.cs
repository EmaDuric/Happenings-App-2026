using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizersController : ControllerBase
    {
        private readonly IOrganizerService _service;

        public OrganizersController(IOrganizerService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
            => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
            => Ok(_service.GetById(id));

        [HttpPost]
        public IActionResult Insert([FromBody] OrganizerInsertRequest request)
            => Ok(_service.Insert(request));

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] OrganizerUpdateRequest request)
            => Ok(_service.Update(id, request));

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
