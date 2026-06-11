using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class LocationUpdateRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string Address { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;
}
