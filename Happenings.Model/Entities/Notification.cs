namespace Happenings.Model.Entities;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string Title { get; set; } = null!;   


    public string Message { get; set; } = null!;
    public bool IsSent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
