using System.ComponentModel.DataAnnotations;

public class InvitationInsertRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Receiver is required")]
    public int ReceiverId { get; set; }
}
