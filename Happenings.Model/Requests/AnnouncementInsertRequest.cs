using System.ComponentModel.DataAnnotations;

public class AnnouncementInsertRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
    public int EventId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 2000 characters")]
    public string Content { get; set; } = null!;
}
