using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTicketTypeController : ControllerBase
{
    private readonly IEventTicketTypeService _service;

    public EventTicketTypeController(IEventTicketTypeService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Insert([FromBody] EventTicketTypeInsertRequest request)
    {
        return Ok(_service.Insert(request));
    }
}
