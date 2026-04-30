using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Happenings.Model;

namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _service;
    public InvitationController(IInvitationService service) => _service = service;

    [HttpGet]
    public async Task<List<InvitationResponse>> Get([FromQuery] int? receiverId)
        => await _service.GetAsync(receiverId);

    [HttpGet("{id}")]
    public async Task<InvitationResponse> GetById(int id)
        => await _service.GetByIdAsync(id);

    [HttpPost]
    public async Task<InvitationResponse> Insert(InvitationInsertRequest request)
        => await _service.InsertAsync(request);

    [HttpPut("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        await _service.UpdateStatusAsync(id, "Accepted");
        return Ok();
    }

    [HttpPut("{id}/decline")]
    public async Task<IActionResult> Decline(int id)
    {
        await _service.UpdateStatusAsync(id, "Declined");
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<bool> Delete(int id) => await _service.DeleteAsync(id);
}