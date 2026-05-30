using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTicketTypeController : ControllerBase
{
    private readonly IEventTicketTypeService _service;
    private readonly IEventService _eventService;

    public EventTicketTypeController(IEventTicketTypeService service, IEventService eventService)
    {
        _service = service;
        _eventService = eventService;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] int eventId) => Ok(_service.GetByEvent(eventId));

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    public IActionResult Insert([FromBody] EventTicketTypeInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EventTicketTypeInsertRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        // Provjeri ownership — ticketType pripada eventu, event pripada organizatoru
        var ticketType = _service.GetById(id);
        if (ticketType == null) return NotFound();

        if (!isAdmin)
        {
            var ev = await _eventService.GetByIdAsync(ticketType.EventId);
            if (ev == null) return NotFound();

            // Provjeri da li je trenutni korisnik organizator tog eventa
            var result = await _eventService.UpdateAsync(ev.Id, null!, userId, false);
            if (result == null) return Forbid();
        }

        return Ok(_service.Update(id, request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organizer,Admin")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}