namespace Happenings.Model.Entities;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public string Location { get; set; } = default!;
}
