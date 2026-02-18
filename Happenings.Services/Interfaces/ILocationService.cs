using Happenings.Model.Requests;
using Happenings.Model.Responses;

namespace Happenings.Services.Interfaces;

public interface ILocationService
{
	List<LocationDto> GetAll();
	LocationDto GetById(int id);
	LocationDto Insert(LocationInsertRequest request);
	LocationDto Update(int id, LocationUpdateRequest request);
	void Delete(int id);
}
