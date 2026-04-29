using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewsController(IReviewService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var review = _service.GetById(id);
            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Insert([FromBody] ReviewInsertRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            request.UserId = int.Parse(userIdClaim.Value);

            return Ok(_service.Insert(request));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ReviewUpdateRequest request)
        {
            return Ok(_service.Update(id, request));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return NoContent();
        }

        [HttpGet("my-reviewed-events")]
        [Authorize]
        public IActionResult GetMyReviewedEvents()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);
            return Ok(_service.GetMyReviewedEvents(userId));
        }

        [HttpGet("eligible-events")]
        [Authorize]
        public IActionResult GetEligibleEvents()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);
            return Ok(_service.GetEligibleEvents(userId));
        }
    }
}
