using Happenings.Model.Requests;
using Happenings.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Happenings.Model;


namespace Happenings.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _service;

    public InvitationController(IInvitationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<List<InvitationResponse>> Get([FromQuery] int? receiverId)
    {
        return await _service.GetAsync(receiverId);
    }

    [HttpGet("{id}")]
    public async Task<InvitationResponse> GetById(int id)
    {
        return await _service.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<InvitationResponse> Insert(InvitationInsertRequest request)
    {
        return await _service.InsertAsync(request);
    }


    [HttpDelete("{id}")]
    public async Task<bool> Delete(int id)
    {
        return await _service.DeleteAsync(id);
    }
}