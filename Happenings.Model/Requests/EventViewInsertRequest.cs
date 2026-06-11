using System.ComponentModel.DataAnnotations;

namespace Happenings.Model.Requests;

public class EventViewInsertRequest
{
    // UserId se uzima iz JWT tokena, ne iz requesta
    [Range(1, int.MaxValue, ErrorMessage = "Event is required")]
    public int EventId { get; set; }
}
