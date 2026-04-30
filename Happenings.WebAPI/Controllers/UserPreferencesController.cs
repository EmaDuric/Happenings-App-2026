using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferenceService _service;
    public UserPreferencesController(IUserPreferenceService service) => _service = service;

    [HttpGet]
    public ActionResult<List<UserPreferenceDto>> Get() => Ok(_service.Get());

    [HttpGet("{id}")]
    public ActionResult<UserPreferenceDto> GetById(int id)
    {
        var result = _service.GetById(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public ActionResult<UserPreferenceDto> Insert(UserPreferenceInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    public ActionResult<UserPreferenceDto> Update(int id, UserPreferenceUpdateRequest request)
    {
        var result = _service.Update(id, request);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}