namespace Happenings.Model.Search;

public class EventSearchObject : BaseSearchObject
{
    public string? Name { get; set; }

    public int? EventCategoryId { get; set; }

    public string? Location { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }


}
