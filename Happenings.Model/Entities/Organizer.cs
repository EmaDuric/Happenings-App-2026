namespace Happenings.Model.Entities;

public class Organizer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
    public string PhoneNumber { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
