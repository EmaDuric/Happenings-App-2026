using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;

public interface IEventTicketTypeService
{
    List<EventTicketTypeDto> GetByEvent(int eventId);
    EventTicketTypeDto Insert(EventTicketTypeInsertRequest request);
   
}