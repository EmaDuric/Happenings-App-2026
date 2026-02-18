using Happenings.Model.Entities;
using Happenings.Model.Requests;
using Happenings.Model.Responses;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;

namespace Happenings.Services.Services;

public class LocationService : ILocationService
{
    private readonly HappeningsContext _context;

    public LocationService(HappeningsContext context)
    {
        _context = context;
    }

    public List<LocationDto> GetAll()
    {
        return _context.Locations
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                City = l.City
            })
            .ToList();
    }

    public LocationDto GetById(int id)
    {
        var entity = _context.Locations.Find(id)
            ?? throw new Exception("Location not found");

        return new LocationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            City = entity.City
        };
    }

    public LocationDto Insert(LocationInsertRequest request)
    {
        var entity = new Location
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City
        };

        _context.Locations.Add(entity);
        _context.SaveChanges();

        return GetById(entity.Id);
    }

    public LocationDto Update(int id, LocationUpdateRequest request)
    {
        var entity = _context.Locations.Find(id)
            ?? throw new Exception("Location not found");

        entity.Name = request.Name;
        entity.Address = request.Address;
        entity.City = request.City;

        _context.SaveChanges();

        return GetById(id);
    }

    public void Delete(int id)
    {
        var entity = _context.Locations.Find(id)
            ?? throw new Exception("Location not found");

        _context.Locations.Remove(entity);
        _context.SaveChanges();
    }
}
