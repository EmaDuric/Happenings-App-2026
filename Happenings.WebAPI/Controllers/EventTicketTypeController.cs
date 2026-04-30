using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTicketTypeController : ControllerBase
{
    private readonly IEventTicketTypeService _service;
    public EventTicketTypeController(IEventTicketTypeService service) => _service = service;

    [HttpGet]
    public IActionResult Get([FromQuery] int eventId) => Ok(_service.GetByEvent(eventId));

    [HttpPost]
    [Authorize]
    public IActionResult Insert([FromBody] EventTicketTypeInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] EventTicketTypeInsertRequest request)
        => Ok(_service.Update(id, request));

    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}