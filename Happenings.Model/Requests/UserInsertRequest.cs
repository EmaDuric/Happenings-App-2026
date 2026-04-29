namespace Happenings.Model.Requests
{
    public class UserInsertRequest
    {
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        // 🔥 ROLE (User vs Organizer)
        public bool IsOrganizer { get; set; } = false;
    }
}