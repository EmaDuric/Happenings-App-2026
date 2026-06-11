using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests
{
    public class OrganizerUpdateRequest
    {
        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
