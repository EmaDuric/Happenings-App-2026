using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class EventImageUpdateRequest
{
    [Required(ErrorMessage = "Image URL is required")]
    [StringLength(1000, ErrorMessage = "Image URL cannot exceed 1000 characters")]
    public string ImageUrl { get; set; } = null!;
}
