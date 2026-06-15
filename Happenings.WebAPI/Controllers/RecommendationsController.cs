using Happenings.Model;
using Happenings.Model.Responses;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _service;
    public RecommendationsController(IRecommendationService service) => _service = service;

    // Korisnik dobija preporuke za sebe — userId iz JWT tokena
    [HttpGet("my")]
    public ActionResult<List<RecommendedEventDto>> GetMyRecommendations()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(_service.GetRecommendedEvents(userId));
    }

    // Admin može dohvatiti preporuke za bilo kojeg korisnika
    [HttpGet("{userId}")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<List<RecommendedEventDto>> GetRecommendations(int userId)
        => Ok(_service.GetRecommendedEvents(userId));
}