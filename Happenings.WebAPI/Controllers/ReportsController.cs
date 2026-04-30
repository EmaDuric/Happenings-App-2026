using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet("event-sales")]
    public IActionResult EventSales() => Ok(_service.GetEventSales());

    [HttpGet("revenue")]
    public IActionResult Revenue() => Ok(_service.GetRevenuePerEvent());

    [HttpGet("ratings")]
    public IActionResult Ratings() => Ok(_service.GetAverageRatingPerEvent());

    [HttpGet("popular-events")]
    public IActionResult GetMostPopularEvents() => Ok(_service.GetMostPopularEvents());
}