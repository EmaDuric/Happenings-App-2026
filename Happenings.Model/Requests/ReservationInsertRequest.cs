namespace Happenings.Model.Requests
{
    public class ReservationInsertRequest
    {

        public int EventId { get; set; }

        public int EventTicketTypeId { get; set; }

        public int Quantity { get; set; }
    }
}
