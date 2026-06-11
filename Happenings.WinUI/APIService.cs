using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Happenings.WinUI
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private string? _authToken;

        public APIService(IConfiguration configuration)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ResolveBaseUrl())
            };
        }

        // Centralno citanje base URL-a iz konfiguracije (ApiBaseUrl). Forme vise ne
        // hardkodiraju http://localhost:5000/api/, nego idu kroz ovaj izvor.
        public static string ResolveBaseUrl()
        {
            var configuration = Program.ServiceProvider?.GetService<IConfiguration>();
            return configuration?["ApiBaseUrl"] ?? "http://localhost:5000/api/";
        }

        // Centralni autorizovani HttpClient koji koriste sve WinUI forme.
        public static HttpClient CreateAuthorizedClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(ResolveBaseUrl()) };
            if (!string.IsNullOrEmpty(TokenStore.Token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            return client;
        }

        private void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var request = new
            {
                email = username,
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

        public async Task<List<EventDto>> GetEventsAsync()
        {
            var response = await _httpClient.GetAsync("Events");
            response.EnsureSuccessStatusCode();
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedResponse<EventDto>>();
            return pagedResult?.Items ?? new List<EventDto>();
        }

        public async Task<EventDto> GetEventByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"Events/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Event not found");
        }

        public async Task<EventDto> CreateEventAsync(EventInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Events", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Failed to create event");
        }

        public async Task<EventDto> UpdateEventAsync(int id, EventUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"Events/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EventDto>() ?? throw new Exception("Failed to update event");
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Events/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("EventCategories");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("EventCategories", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Failed to create category");
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"EventCategories/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Failed to update category");
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"EventCategories/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<LocationDto>> GetLocationsAsync()
        {
            var response = await _httpClient.GetAsync("Locations");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LocationDto>>() ?? new List<LocationDto>();
        }

        public async Task<LocationDto> CreateLocationAsync(LocationInsertRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("Locations", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LocationDto>() ?? throw new Exception("Failed to create location");
        }

        public async Task<LocationDto> UpdateLocationAsync(int id, LocationUpdateRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"Locations/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LocationDto>() ?? throw new Exception("Failed to update location");
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"Locations/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<OrganizerDto>> GetOrganizersAsync()
        {
            var response = await _httpClient.GetAsync("Organizers");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<OrganizerDto>>() ?? new List<OrganizerDto>();
        }
    }

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
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string EventCategoryName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int OrganizerId { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EventInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public int LocationId { get; set; }
    }

    public class EventUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int EventCategoryId { get; set; }
        public int LocationId { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryUpdateRequest : CategoryInsertRequest { }

    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    public class LocationInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    public class LocationUpdateRequest : LocationInsertRequest { }

    public class OrganizerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class PagedResponse<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}