namespace Happenings.Model.Requests
{
    public class ReviewInsertRequest
    {
        public int Rating { get; set; } // 1–5
        public string? Comment { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
    }
}
