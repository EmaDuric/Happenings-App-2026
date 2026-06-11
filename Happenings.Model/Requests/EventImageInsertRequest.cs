using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class EventImageInsertRequest
{
    [Required(ErrorMessage = "Image URL is required")]
    [StringLength(1000, ErrorMessage = "Image URL cannot exceed 1000 characters")]
    public string ImageUrl { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
    public int EventId { get; set; }
}
