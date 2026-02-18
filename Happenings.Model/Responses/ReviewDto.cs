namespace Happenings.Model.Responses
{
    public class ReviewDto
    {
        public int Id { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
    }
}
