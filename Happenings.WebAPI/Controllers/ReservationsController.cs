using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    public ReservationsController(IReservationService service) => _service = service;

    // ADMIN — lista svih rezervacija
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get() => Ok(_service.Get());

    // Korisnik vidi samo svoju rezervaciju
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");
        var result = _service.GetById(id, userId, isAdmin);
        if (result == null) return Forbid();
        return Ok(result);
    }

    // Moje rezervacije iz JWT tokena
    [HttpGet("my")]
    public IActionResult GetMy()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetByUserId(userId));
    }

    [HttpPost]
    public IActionResult Insert([FromBody] ReservationInsertRequest request)
        => Ok(_service.Insert(request));

    // Korisnik može mijenjati samo svoju rezervaciju
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ReservationUpdateRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");
        var result = _service.Update(id, request, userId, isAdmin);
        if (result == null) return Forbid();
        return Ok(result);
    }

    // Soft delete — postavlja status na Cancelled
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");
        var result = _service.Cancel(id, userId, isAdmin);
        if (!result) return Forbid();
        return NoContent();
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