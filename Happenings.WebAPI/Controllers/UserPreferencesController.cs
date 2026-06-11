using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferenceService _service;
    public UserPreferencesController(IUserPreferenceService service) => _service = service;

    private int CurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // Admin � sve preference
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<List<UserPreferenceDto>> Get() => Ok(_service.Get());

    // Korisnik vidi svoje preference � userId iz JWT tokena
    [HttpGet("my")]
    public ActionResult<List<UserPreferenceDto>> GetMy()
        => Ok(_service.GetByUser(CurrentUserId()));

    [HttpGet("{id}")]
    public ActionResult<UserPreferenceDto> GetById(int id)
    {
        var result = _service.GetById(id, CurrentUserId(), User.IsInRole("Admin"));
        return result == null ? Forbid() : Ok(result);
    }

    // userId se uzima iz JWT tokena � ne iz requesta
    [HttpPost]
    public ActionResult<UserPreferenceDto> Insert(UserPreferenceInsertRequest request)
        => Ok(_service.Insert(request, CurrentUserId()));

    [HttpPut("{id}")]
    public ActionResult<UserPreferenceDto> Update(int id, UserPreferenceUpdateRequest request)
    {
        var result = _service.Update(id, request, CurrentUserId(), User.IsInRole("Admin"));
        return result == null ? Forbid() : Ok(result);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (!_service.Delete(id, CurrentUserId(), User.IsInRole("Admin"))) return Forbid();
        return NoContent();
    }
}