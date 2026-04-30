using Happenings.WinUI.Models;
using System.Collections.Generic;

namespace Happenings.WinUI.Services
{
    public static class MockDataService
    {
        private static List<CategoryViewModel>? _categories;
        private static List<VenueViewModel>? _venues;
        private static List<OrganizerViewModel>? _organizers;

        // CATEGORIES
        public static List<CategoryViewModel> GetCategories()
        {
            if (_categories == null)
            {
                _categories = new List<CategoryViewModel>
                {
                    new CategoryViewModel { Id = 1, Name = "Music", Description = "Music concerts and festivals", Icon = "🎵", IsActive = true },
                    new CategoryViewModel { Id = 2, Name = "Sports", Description = "Sports events and competitions", Icon = "⚽", IsActive = true },
                    new CategoryViewModel { Id = 3, Name = "Art", Description = "Art exhibitions and galleries", Icon = "🎨", IsActive = true },
                    new CategoryViewModel { Id = 4, Name = "Conference", Description = "Business and tech conferences", Icon = "💼", IsActive = true },
                    new CategoryViewModel { Id = 5, Name = "Theater", Description = "Theater performances and plays", Icon = "🎭", IsActive = true },
                    new CategoryViewModel { Id = 6, Name = "Comedy", Description = "Stand-up comedy shows", Icon = "😂", IsActive = true },
                    new CategoryViewModel { Id = 7, Name = "Food & Drink", Description = "Food festivals and tastings", Icon = "🍕", IsActive = true }
                };
            }
            return _categories;
        }

        public static void AddCategory(CategoryViewModel category)
        {
            if (_categories == null) GetCategories();
            category.Id = _categories!.Count > 0 ? _categories.Max(c => c.Id) + 1 : 1;
            _categories.Add(category);
        }

        public static void UpdateCategory(CategoryViewModel category)
        {
            var existing = _categories?.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.Icon = category.Icon;
                existing.IsActive = category.IsActive;
            }
        }

        public static void DeleteCategory(int id)
        {
            var category = _categories?.FirstOrDefault(c => c.Id == id);
            if (category != null) _categories?.Remove(category);
        }

        // VENUES
        public static List<VenueViewModel> GetVenues()
        {
            if (_venues == null)
            {
                _venues = new List<VenueViewModel>
                {
                    new VenueViewModel { Id = 1, Name = "Central Park Arena", Address = "123 Park Avenue", City = "New York", Capacity = 5000, Type = "Outdoor", IsActive = true },
                    new VenueViewModel { Id = 2, Name = "Convention Center", Address = "456 Business Blvd", City = "Chicago", Capacity = 2000, Type = "Indoor", IsActive = true },
                    new VenueViewModel { Id = 3, Name = "City Art Gallery", Address = "789 Arts Street", City = "Los Angeles", Capacity = 500, Type = "Indoor", IsActive = true },
                    new VenueViewModel { Id = 4, Name = "National Stadium", Address = "321 Sports Way", City = "Boston", Capacity = 50000, Type = "Stadium", IsActive = true },
                    new VenueViewModel { Id = 5, Name = "Riverside Park", Address = "555 River Road", City = "Seattle", Capacity = 1000, Type = "Outdoor", IsActive = true },
                    new VenueViewModel { Id = 6, Name = "Grand Theater", Address = "777 Theater Lane", City = "San Francisco", Capacity = 800, Type = "Theater", IsActive = true }
                };
            }
            return _venues;
        }

        public static void AddVenue(VenueViewModel venue)
        {
            if (_venues == null) GetVenues();
            venue.Id = _venues!.Count > 0 ? _venues.Max(v => v.Id) + 1 : 1;
            _venues.Add(venue);
        }

        public static void UpdateVenue(VenueViewModel venue)
        {
            var existing = _venues?.FirstOrDefault(v => v.Id == venue.Id);
            if (existing != null)
            {
                existing.Name = venue.Name;
                existing.Address = venue.Address;
                existing.City = venue.City;
                existing.Capacity = venue.Capacity;
                existing.Type = venue.Type;
                existing.IsActive = venue.IsActive;
            }
        }

        public static void DeleteVenue(int id)
        {
            var venue = _venues?.FirstOrDefault(v => v.Id == id);
            if (venue != null) _venues?.Remove(venue);
        }

        // ORGANIZERS
        public static List<OrganizerViewModel> GetOrganizers()
        {
            if (_organizers == null)
            {
                _organizers = new List<OrganizerViewModel>
                {
                    new OrganizerViewModel { Id = 1, Name = "Event Masters Inc.", ContactPerson = "John Smith", Email = "john@eventmasters.com", Phone = "+1-555-0101", IsActive = true },
                    new OrganizerViewModel { Id = 2, Name = "Tech Events LLC", ContactPerson = "Sarah Johnson", Email = "sarah@techevents.com", Phone = "+1-555-0102", IsActive = true },
                    new OrganizerViewModel { Id = 3, Name = "Arts Council", ContactPerson = "Michael Brown", Email = "michael@artscouncil.org", Phone = "+1-555-0103", IsActive = true },
                    new OrganizerViewModel { Id = 4, Name = "Sports Federation", ContactPerson = "Emily Davis", Email = "emily@sportsfed.com", Phone = "+1-555-0104", IsActive = true },
                    new OrganizerViewModel { Id = 5, Name = "Theater Company", ContactPerson = "David Wilson", Email = "david@theaterco.com", Phone = "+1-555-0105", IsActive = true }
                };
            }
            return _organizers;
        }

        public static void AddOrganizer(OrganizerViewModel organizer)
        {
            if (_organizers == null) GetOrganizers();
            organizer.Id = _organizers!.Count > 0 ? _organizers.Max(o => o.Id) + 1 : 1;
            _organizers.Add(organizer);
        }

        public static void UpdateOrganizer(OrganizerViewModel organizer)
        {
            var existing = _organizers?.FirstOrDefault(o => o.Id == organizer.Id);
            if (existing != null)
            {
                existing.Name = organizer.Name;
                existing.ContactPerson = organizer.ContactPerson;
                existing.Email = organizer.Email;
                existing.Phone = organizer.Phone;
                existing.IsActive = organizer.IsActive;
            }
        }

        public static void DeleteOrganizer(int id)
        {
            var organizer = _organizers?.FirstOrDefault(o => o.Id == id);
            if (organizer != null) _organizers?.Remove(organizer);
        }
    }
}