public class InvitationResponse
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public string EventName { get; set; }

    public int SenderId { get; set; }
    public string SenderName { get; set; }

    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; }

    public string Status { get; set; }

    public DateTime SentAt { get; set; }
}