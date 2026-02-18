namespace Happenings.Model.Requests;

public class UserPreferenceUpdateRequest
{
    public string PreferenceType { get; set; } = null!;
    public string PreferenceValue { get; set; } = null!;
}
