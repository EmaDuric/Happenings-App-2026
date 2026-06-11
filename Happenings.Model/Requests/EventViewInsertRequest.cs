namespace Happenings.Model.Requests;

public class EventViewInsertRequest
{
    // UserId se uzima iz JWT tokena, ne iz requesta
    public int EventId { get; set; }
}
