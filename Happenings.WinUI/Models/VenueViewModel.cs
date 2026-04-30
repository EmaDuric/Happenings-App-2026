namespace Happenings.WinUI.Models
{
	public class VenueViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public int Capacity { get; set; }
		public string Type { get; set; } = string.Empty; // Indoor, Outdoor, Stadium, Theater
		public bool IsActive { get; set; } = true;
	}
}