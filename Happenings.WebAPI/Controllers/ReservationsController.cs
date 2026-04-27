using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers
{
	[ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
	{
		private readonly IReservationService _service;

		public ReservationsController(IReservationService service)
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
        [Authorize]
        public IActionResult Insert([FromBody] ReservationInsertRequest request)
			=> Ok(_service.Insert(request));

		[HttpPut("{id}")]
		public IActionResult Update(int id, [FromBody] ReservationUpdateRequest request)
			=> Ok(_service.Update(id, request));

		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			_service.Delete(id);
			return NoContent();
		}

        [HttpPost("{id}/approve")]
        public IActionResult Approve(int id)
        {
            _service.Approve(id);
            return Ok();
        }

        [HttpPost("{id}/reject")]
        public IActionResult Reject(int id)
        {
            _service.Reject(id);
            return Ok();
        }
    }
}
