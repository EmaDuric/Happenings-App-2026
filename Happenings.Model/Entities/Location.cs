namespace Happenings.Model.Entities;

public class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
