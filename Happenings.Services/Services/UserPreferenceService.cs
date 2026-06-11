using Happenings.Model.DTOs;
using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;

namespace Happenings.Services.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly HappeningsContext _context;

    public UserPreferenceService(HappeningsContext context)
    {
        _context = context;
    }

    public List<UserPreferenceDto> Get()
    {
        return _context.UserPreferences
            .AsEnumerable()
            .Select(MapToDto)
            .ToList();
    }

    public List<UserPreferenceDto> GetByUser(int userId)
    {
        return _context.UserPreferences
            .Where(x => x.UserId == userId)
            .AsEnumerable()
            .Select(MapToDto)
            .ToList();
    }

    public UserPreferenceDto? GetById(int id, int userId, bool isAdmin)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return null;
        if (!isAdmin && entity.UserId != userId) return null;

        return MapToDto(entity);
    }

    public UserPreferenceDto Insert(UserPreferenceInsertRequest request, int userId)
    {
        var entity = new UserPreference
        {
            UserId = userId, // iz JWT-a, ne iz requesta
            PreferenceType = request.PreferenceType,
            PreferenceValue = request.PreferenceValue
        };

        _context.UserPreferences.Add(entity);
        _context.SaveChanges();

        return MapToDto(entity);
    }

    public UserPreferenceDto? Update(int id, UserPreferenceUpdateRequest request, int userId, bool isAdmin)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return null;
        if (!isAdmin && entity.UserId != userId) return null;

        entity.PreferenceType = request.PreferenceType;
        entity.PreferenceValue = request.PreferenceValue;

        _context.SaveChanges();

        return MapToDto(entity);
    }

    public bool Delete(int id, int userId, bool isAdmin)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return false;
        if (!isAdmin && entity.UserId != userId) return false;

        _context.UserPreferences.Remove(entity);
        _context.SaveChanges();

        return true;
    }

    private static UserPreferenceDto MapToDto(UserPreference x) => new()
    {
        Id = x.Id,
        UserId = x.UserId,
        PreferenceType = x.PreferenceType,
        PreferenceValue = x.PreferenceValue
    };
}
