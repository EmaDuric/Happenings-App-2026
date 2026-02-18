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
            .Select(x => new UserPreferenceDto
            {
                Id = x.Id,
                UserId = x.UserId,
                PreferenceType = x.PreferenceType,
                PreferenceValue = x.PreferenceValue
            }).ToList();
    }

    public UserPreferenceDto? GetById(int id)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return null;

        return new UserPreferenceDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PreferenceType = entity.PreferenceType,
            PreferenceValue = entity.PreferenceValue
        };
    }

    public UserPreferenceDto Insert(UserPreferenceInsertRequest request)
    {
        var entity = new UserPreference
        {
            UserId = request.UserId,
            PreferenceType = request.PreferenceType,
            PreferenceValue = request.PreferenceValue
        };

        _context.UserPreferences.Add(entity);
        _context.SaveChanges();

        return new UserPreferenceDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PreferenceType = entity.PreferenceType,
            PreferenceValue = entity.PreferenceValue
        };
    }

    public UserPreferenceDto? Update(int id, UserPreferenceUpdateRequest request)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return null;

        entity.PreferenceType = request.PreferenceType;
        entity.PreferenceValue = request.PreferenceValue;

        _context.SaveChanges();

        return new UserPreferenceDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            PreferenceType = entity.PreferenceType,
            PreferenceValue = entity.PreferenceValue
        };
    }

    public bool Delete(int id)
    {
        var entity = _context.UserPreferences.Find(id);
        if (entity == null) return false;

        _context.UserPreferences.Remove(entity);
        _context.SaveChanges();

        return true;
    }
}
