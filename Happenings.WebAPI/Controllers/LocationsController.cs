using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _service;
    public LocationsController(ILocationService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(int id) => Ok(_service.GetById(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Insert(LocationInsertRequest request) => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(int id, LocationUpdateRequest request) => Ok(_service.Update(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}