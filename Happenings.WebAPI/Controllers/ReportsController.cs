using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("event-sales")]
    public IActionResult EventSales()
    {
        return Ok(_service.GetEventSales());
    }

    [HttpGet("revenue")]
    public IActionResult Revenue()
    {
        return Ok(_service.GetRevenuePerEvent());
    }

    [HttpGet("ratings")]
    public IActionResult Ratings()
    {
        return Ok(_service.GetAverageRatingPerEvent());
    }

    [HttpGet("popular-events")]
    public IActionResult GetMostPopularEvents()
    {
        var result = _service.GetMostPopularEvents();

        return Ok(result);
    }
}