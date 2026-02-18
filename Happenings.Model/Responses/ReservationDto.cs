namespace Happenings.Model.Responses
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public DateTime ReservedAt { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
    }
}
