namespace Happenings.Model.Responses;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = null!;
    public bool IsSent { get; set; }
    public DateTime CreatedAt { get; set; }
}
