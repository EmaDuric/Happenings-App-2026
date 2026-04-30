using System;

namespace Happenings.WinUI.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }

        // IDs umjesto stringova
        public int CategoryId { get; set; }
        public int VenueId { get; set; }
        public int OrganizerId { get; set; }

        public decimal Price { get; set; }
        public int AvailableTickets { get; set; }
        public int TotalTickets { get; set; }
        public string Status { get; set; } = "Active";

        // Display properties (za prikaz u grid-u)
        public string Category { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string Organizer { get; set; } = string.Empty;

        public string FormattedDate => StartDateTime.ToString("dd.MM.yyyy HH:mm");
        public string FormattedPrice => $"${Price:F2}";
        public string TicketInfo => $"{AvailableTickets}/{TotalTickets}";
    }
}