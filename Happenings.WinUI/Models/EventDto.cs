using System;

namespace Happenings.WinUI.Models
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public string EventCategoryName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        // Uklonjena stara polja koja server EventDto ne vraca:
        // StartDate, EndDate, Price, TotalTickets, AvailableTickets.
    }
}