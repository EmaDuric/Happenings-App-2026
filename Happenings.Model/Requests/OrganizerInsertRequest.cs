using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests
{
    public class OrganizerInsertRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
    }
}
