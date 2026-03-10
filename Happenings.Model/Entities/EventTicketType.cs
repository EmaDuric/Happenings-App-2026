namespace Happenings.Model.Entities;

public class EventTicketType
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public string Name { get; set; } = null!; // VIP, Regular

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }
}