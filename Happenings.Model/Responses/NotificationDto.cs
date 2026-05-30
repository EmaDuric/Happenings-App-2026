public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }  // ← dodaj
    public DateTime CreatedAt { get; set; }
}