using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;

public interface IEventTicketTypeService
{
    List<EventTicketTypeDto> GetByEvent(int eventId);
    EventTicketTypeDto? GetById(int id);

    // Ownership-aware operacije: vracaju null (Insert/Update) ili false (Delete)
    // kada korisnik nije admin niti organizator eventa kojem tip ulaznice pripada.
    EventTicketTypeDto? Insert(EventTicketTypeInsertRequest request, int userId, bool isAdmin);
    EventTicketTypeDto? Update(int id, EventTicketTypeInsertRequest request, int userId, bool isAdmin);
    bool Delete(int id, int userId, bool isAdmin);
}