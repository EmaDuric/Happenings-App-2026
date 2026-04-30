namespace Happenings.Model.Entities;

public class Invitation
{
	public int Id { get; set; }

	public int EventId { get; set; }
	public Event Event { get; set; }

	public int SenderId { get; set; }
	public User Sender { get; set; }

	public int ReceiverId { get; set; }
	public User Receiver { get; set; }

	public string Status { get; set; } // Pending, Accepted, Rejected

	public DateTime SentAt { get; set; }
}