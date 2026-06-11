using System.ComponentModel.DataAnnotations;

public class EventTicketTypeInsertRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
    public int EventId { get; set; }

    [Required(ErrorMessage = "Ticket type name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = null!;

    [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000")]
    public decimal Price { get; set; }

    [Range(1, 100000, ErrorMessage = "Available quantity must be between 1 and 100000")]
    public int AvailableQuantity { get; set; }
}