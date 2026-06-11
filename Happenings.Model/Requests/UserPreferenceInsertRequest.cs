namespace Happenings.Model.Requests;

public class UserPreferenceInsertRequest
{
    // UserId se uzima iz JWT tokena, ne iz requesta
    public string PreferenceType { get; set; } = null!;
    public string PreferenceValue { get; set; } = null!;
}
