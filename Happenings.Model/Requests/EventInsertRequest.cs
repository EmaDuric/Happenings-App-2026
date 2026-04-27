namespace Happenings.Model.Requests;

public class EventInsertRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime EventDate { get; set; }

    public int EventCategoryId { get; set; }
    public int LocationId { get; set; }

    // Ne šalje se iz Fluttera, puni se iz JWT-a u controlleru
    public int OrganizerId { get; set; }
}