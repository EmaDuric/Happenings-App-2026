using Microsoft.AspNetCore.Mvc;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventImagesController : ControllerBase
    {
        private readonly IEventImageService _service;

        public EventImagesController(IEventImageService service)
        {
            _service = service;
        }

        // GET: api/EventImages
        [HttpGet]
        public ActionResult<List<EventImageDto>> GetAll()
        {
            return Ok(_service.GetAll());
        }

        // GET: api/EventImages/5
        [HttpGet("{id}")]
        public ActionResult<EventImageDto> GetById(int id)
        {
            var result = _service.GetById(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/EventImages/by-event/10
        [HttpGet("by-event/{eventId}")]
        public ActionResult<List<EventImageDto>> GetByEvent(int eventId)
        {
            return Ok(_service.GetByEvent(eventId));
        }

        // POST: api/EventImages
        [HttpPost]
        public ActionResult<EventImageDto> Insert(EventImageInsertRequest request)
        {
            var result = _service.Insert(request);
            return Ok(result);
        }

        // PUT: api/EventImages/5
        [HttpPut("{id}")]
        public ActionResult<EventImageDto> Update(int id, EventImageUpdateRequest request)
        {
            var result = _service.Update(id, request);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // DELETE: api/EventImages/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _service.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
