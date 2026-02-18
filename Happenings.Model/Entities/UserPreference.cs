namespace Happenings.Model.Entities;

public class UserPreference
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string PreferenceType { get; set; } = null!;
    public string PreferenceValue { get; set; } = null!;
}
