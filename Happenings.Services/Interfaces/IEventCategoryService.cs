using Happenings.Model.DTOs;
using Happenings.Model.Requests;

namespace Happenings.Services.Interfaces;

public interface IEventCategoryService
{
	List<EventCategoryDto> Get();

	EventCategoryDto? GetById(int id);

	EventCategoryDto Insert(EventCategoryInsertRequest request);

	EventCategoryDto Update(int id, EventCategoryUpdateRequest request);

	bool Delete(int id);
}
