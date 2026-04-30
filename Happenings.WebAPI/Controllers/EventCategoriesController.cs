using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventCategoriesController : ControllerBase
{
    private readonly IEventCategoryService _service;
    public EventCategoriesController(IEventCategoryService service) => _service = service;

    [HttpGet]
    public ActionResult<List<EventCategoryDto>> Get() => Ok(_service.Get());

    [HttpGet("{id}")]
    public ActionResult<EventCategoryDto> GetById(int id)
    {
        var result = _service.GetById(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<EventCategoryDto> Insert(EventCategoryInsertRequest request)
        => Ok(_service.Insert(request));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public ActionResult<EventCategoryDto> Update(int id, EventCategoryUpdateRequest request)
        => Ok(_service.Update(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public ActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}