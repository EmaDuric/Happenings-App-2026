namespace Happenings.Model.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        // 🔥 ROLE
        public bool IsOrganizer { get; set; } = false;

        // 🔥 1:1 veza sa Organizer
        public Organizer? Organizer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public bool IsAdmin { get; set; } = false;

        public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    }
}