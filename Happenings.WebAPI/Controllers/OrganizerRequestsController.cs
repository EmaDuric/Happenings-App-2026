using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizerRequestsController : ControllerBase
{
    private readonly IOrganizerRequestService _service;

    public OrganizerRequestsController(IOrganizerRequestService service)
    {
        _service = service;
    }

    // Korisnik šalje zahtjev — userId iz JWT
    [HttpPost]
    public async Task<IActionResult> Insert()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _service.InsertAsync(userId);
        return Ok(result);
    }

    // Admin vidi sve zahtjeve
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAll() => Ok(_service.GetAll());

    // Admin odobrava zahtjev
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _service.ApproveAsync(id, adminId);
        return Ok(new { message = "Request approved successfully" });
    }

    // Admin odbija zahtjev
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, [FromBody] OrganizerRequestRejectRequest request)
    {
        var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _service.RejectAsync(id, adminId, request.Reason);
        return Ok(new { message = "Request rejected" });
    }
}