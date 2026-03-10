namespace Happenings.Model.Requests;

public class NotificationInsertRequest
{
    public string Message { get; set; } = null!;
    public int UserId { get; set; }
    public string Title { get; set; } = null!;

}
