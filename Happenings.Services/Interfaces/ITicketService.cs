using Happenings.Model.DTOs;
using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;


public interface ITicketService
{
    List<TicketDto> Get();
    TicketDto Insert(TicketInsertRequest request);
    TicketDto? GetById(int id);
}
