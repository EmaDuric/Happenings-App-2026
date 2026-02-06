using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new[]
            {
                new { Id = 1, Name = "RS2 test event", Location = "Mostar" }
            });
        }
    }
}
