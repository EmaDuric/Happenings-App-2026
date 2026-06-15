using Happenings.Model.Requests;
using Happenings.Model;
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
    public EventsController(IEventService service) => _service = service;

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
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Insert([FromBody] EventInsertRequest request)
        => Ok(await _service.InsertAsync(request));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);
        var result = await _service.UpdateAsync(id, request, userId, isAdmin);
        if (result == null) return Forbid();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);
        var result = await _service.DeleteAsync(id, userId, isAdmin);
        if (!result) return Forbid();
        return Ok(result);
    }
}