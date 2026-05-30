using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests
{
    public class ReservationInsertRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
        public int EventId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ticket type is required")]
        public int EventTicketTypeId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
}