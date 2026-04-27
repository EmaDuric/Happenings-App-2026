namespace Happenings.Model.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsOrganizer { get; set; }
        public Organizer? Organizer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    }
}
