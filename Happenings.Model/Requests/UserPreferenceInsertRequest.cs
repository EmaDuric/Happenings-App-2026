namespace Happenings.Model.Requests;

public class UserPreferenceInsertRequest
{
    public int UserId { get; set; }
    public string PreferenceType { get; set; } = null!;
    public string PreferenceValue { get; set; } = null!;
}
