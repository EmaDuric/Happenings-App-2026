using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Happenings.WinUI
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private string? _authToken;
        private const string BASE_URL = "http://localhost:5000/api/";

        public APIService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL)
            };
        }

        // HELPER: Set Auth Token
        private void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // AUTH: Login
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var request = new
            {
                email = username,  // ← Backend očekuje "email" ne "username"
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync("Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                SetAuthToken(result.Token);
            }

            return result!;
        }

        // EVENTS: Get All
        // EVENTS: Get All
        public async Task<List<EventDto>> GetEventsAsync()
        {
            var response = await _httpClient.GetAsync("Events");
            response.EnsureSuccessStatusCode();

            // API vraća PagedResponse, ne direktno List
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedResponse<EventDto>>();

            return pagedResult?.Items ?? new List<EventDto>();
        }

        // EVENTS: Get By ID
        public async Task<EventDto> GetEventByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Events/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Event not found");
        }

        // EVENTS: Create
        public async Task<EventDto> CreateEventAsync(EventInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Events", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Failed to create event");
        }

        // EVENTS: Update
        public async Task<EventDto> UpdateEventAsync(int id, EventUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"Events/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Failed to update event");
        }

        // EVENTS: Delete
        public async Task<bool> DeleteEventAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Events/{id}");
            return response.IsSuccessStatusCode;
        }

        // CATEGORIES: Get All
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("EventCategories");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();
        }

        // CATEGORIES: Create
        public async Task<CategoryDto> CreateCategoryAsync(CategoryInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("EventCategories", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Failed to create category");
        }

        // CATEGORIES: Update
        public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"EventCategories/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Failed to update category");
        }

        // CATEGORIES: Delete
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"EventCategories/{id}");
            return response.IsSuccessStatusCode;
        }

        // LOCATIONS: Get All
        public async Task<List<LocationDto>> GetLocationsAsync()
        {
            var response = await _httpClient.GetAsync("Locations");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LocationDto>>() ?? new List<LocationDto>();
        }

        // LOCATIONS: Create
        public async Task<LocationDto> CreateLocationAsync(LocationInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Locations", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LocationDto>() ?? throw new Exception("Failed to create location");
        }

        // LOCATIONS: Update
        public async Task<LocationDto> UpdateLocationAsync(int id, LocationUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"Locations/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LocationDto>() ?? throw new Exception("Failed to update location");
        }

        // LOCATIONS: Delete
        public async Task<bool> DeleteLocationAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Locations/{id}");
            return response.IsSuccessStatusCode;
        }

        // ORGANIZERS: Get All
        public async Task<List<OrganizerDto>> GetOrganizersAsync()
        {
            var response = await _httpClient.GetAsync("Organizers");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<OrganizerDto>>() ?? new List<OrganizerDto>();
        }
    }

    // DTO MODELS (basic structure - adjust based on your actual models)

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }        // ← dodaj
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int EventCategoryId { get; set; }
        public string EventCategoryName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;  // ← dodaj
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EventInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int EventCategoryId { get; set; }
        public int LocationId { get; set; }
        public int OrganizerId { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
    }

    public class EventUpdateRequest : EventInsertRequest
    {
        public int AvailableTickets { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CategoryInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class CategoryUpdateRequest : CategoryInsertRequest
    {
        public bool IsActive { get; set; }
    }

    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class LocationInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class LocationUpdateRequest : LocationInsertRequest
    {
    }

    public class OrganizerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
    public class PagedResponse<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}