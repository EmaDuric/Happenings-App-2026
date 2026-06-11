using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class UserPreferenceUpdateRequest
{
    [Required(ErrorMessage = "Preference type is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Preference type must be between 1 and 100 characters")]
    public string PreferenceType { get; set; } = null!;

    [Required(ErrorMessage = "Preference value is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Preference value must be between 1 and 500 characters")]
    public string PreferenceValue { get; set; } = null!;
}
