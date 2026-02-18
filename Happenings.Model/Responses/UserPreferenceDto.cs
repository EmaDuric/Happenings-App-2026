namespace Happenings.Model.DTOs;

public class UserPreferenceDto
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string PreferenceType { get; set; } = null!;
	public string PreferenceValue { get; set; } = null!;
}
