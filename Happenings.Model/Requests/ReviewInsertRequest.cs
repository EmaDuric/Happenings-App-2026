using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests
{
    public class ReviewInsertRequest
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }

        // UserId se postavlja iz JWT tokena u controlleru, ne prima od klijenta
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
        public int EventId { get; set; }
    }
}