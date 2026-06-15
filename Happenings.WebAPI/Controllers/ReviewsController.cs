using Happenings.Model;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    public ReviewsController(IReviewService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var review = _service.GetById(id);
        return review == null ? NotFound() : Ok(review);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Insert([FromBody] ReviewInsertRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);
        request.UserId = userId;

        // Greske (BusinessRule/Conflict/...) hvata ExceptionMiddleware po tipu
        return Ok(_service.Insert(request));
    }

    // Korisnik mo�e mijenjati samo svoju recenziju
    [HttpPut("{id}")]
    [Authorize]
    public IActionResult Update(int id, [FromBody] ReviewUpdateRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);

        var result = _service.Update(id, request, userId, isAdmin);
        if (result == null) return Forbid();
        return Ok(result);
    }

    // Korisnik mo�e brisati samo svoju recenziju
    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole(Roles.Admin);

        var result = _service.Delete(id, userId, isAdmin);
        if (!result) return Forbid();
        return NoContent();
    }

    [HttpGet("my-reviewed-events")]
    [Authorize]
    public IActionResult GetMyReviewedEvents()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetMyReviewedEvents(userId));
    }

    [HttpGet("eligible-events")]
    [Authorize]
    public IActionResult GetEligibleEvents()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetEligibleEvents(userId));
    }
}