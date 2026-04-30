using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    public ReservationsController(IReservationService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get() => Ok(_service.Get());

    [HttpGet("{id}")]
    public IActionResult GetById(int id) => Ok(_service.GetById(id));

    [HttpPost]
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
    [HttpGet("my")]
    public IActionResult GetMy()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);
        return Ok(_service.GetByUserId(userId));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public IActionResult Approve(int id)
    {
        _service.Approve(id);
        return Ok();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public IActionResult Reject(int id)
    {
        _service.Reject(id);
        return Ok();
    }
}