using Happenings.Model.Responses;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _service;

    public RecommendationsController(IRecommendationService service)
    {
        _service = service;
    }

    [HttpGet("{userId}")]
    public ActionResult<List<RecommendedEventDto>> GetRecommendations(int userId)
    {
        return Ok(_service.GetRecommendedEvents(userId));
    }
}