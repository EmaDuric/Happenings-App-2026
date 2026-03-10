public class EventTicketTypeDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
}