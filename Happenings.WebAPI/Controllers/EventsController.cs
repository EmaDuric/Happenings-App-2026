using Happenings.Model.Requests;
using Happenings.Model.Search;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] EventSearchObject search)
        => Ok(await _service.GetAsync(search));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Insert([FromBody] EventInsertRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        request.OrganizerId = int.Parse(userIdClaim.Value);

        return Ok(await _service.InsertAsync(request));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => Ok(await _service.DeleteAsync(id));
}
