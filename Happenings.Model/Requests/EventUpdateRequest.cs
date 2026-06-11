using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class EventUpdateRequest
{
    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Event date is required")]
    public DateTime EventDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int EventCategoryId { get; set; }

    // OrganizerId se odreduje iz JWT-a/ownership provjere, ne validira se ovdje
    public int OrganizerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Location is required")]
    public int LocationId { get; set; }
}
