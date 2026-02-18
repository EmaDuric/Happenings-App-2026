using Happenings.Model.Responses;
using Happenings.Model.Search;

namespace Happenings.Services.Interfaces;

public interface ICRUDService<TDto, TSearch, TInsert, TUpdate>
    where TSearch : BaseSearchObject
{
    Task<PagedResult<TDto>> GetAsync(TSearch search);
    Task<TDto?> GetByIdAsync(int id);
    Task<TDto> InsertAsync(TInsert request);
    Task<TDto> UpdateAsync(int id, TUpdate request);
    Task<bool> DeleteAsync(int id);
}
