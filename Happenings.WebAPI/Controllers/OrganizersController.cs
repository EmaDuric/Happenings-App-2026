using Happenings.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Services.Interfaces;
using Happenings.Model.Requests;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizersController : ControllerBase
{
    private readonly IOrganizerService _service;

    public OrganizersController(IOrganizerService service) => _service = service;

    [HttpGet]
    [Authorize]
    public IActionResult Get() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetById(int id) => Ok(_service.GetById(id));

    // Samo Admin može kreirati organizatora
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Insert([FromBody] OrganizerInsertRequest request)
        => Ok(_service.Insert(request));

    // Samo Admin može mijenjati
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Update(int id, [FromBody] OrganizerUpdateRequest request)
        => Ok(_service.Update(id, request));

    // Samo Admin može brisati
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}