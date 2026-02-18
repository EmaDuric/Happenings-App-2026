using Happenings.Model.Responses;
using Happenings.Model.Search;
using Happenings.Services.Database;
using Happenings.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Happenings.Services.Services;

public abstract class BaseCRUDService<TEntity, TDto, TSearch, TInsert, TUpdate>
    : ICRUDService<TDto, TSearch, TInsert, TUpdate>
    where TEntity : class
    where TSearch : BaseSearchObject
{
    protected readonly HappeningsContext _context;
    protected readonly DbSet<TEntity> _set;

    protected BaseCRUDService(HappeningsContext context)
    {
        _context = context;
        _set = _context.Set<TEntity>();
    }

    // 1) Query builder – override u konkretnim servisima za filtere/include
    protected virtual IQueryable<TEntity> BuildQuery(TSearch search)
        => _set.AsQueryable();

    // 2) Mapiranja – implementiraš u konkretnom servisu
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract TEntity MapInsertToEntity(TInsert request);
    protected abstract void MapUpdateToEntity(TUpdate request, TEntity entity);

    public async Task<PagedResult<TDto>> GetAsync(TSearch search)
    {
        var query = BuildQuery(search);

        var page = search.Page ?? 1;
        var pageSize = search.PageSize ?? 20;
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var totalCount = await query.CountAsync();

        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TDto>
        {
            TotalCount = totalCount,
            Items = list.Select(MapToDto).ToList()
        };
    }

    public async Task<TDto?> GetByIdAsync(int id)
    {
        var entity = await FindByIdAsync(id);
        return entity == null ? default : MapToDto(entity);
    }

    public async Task<TDto> InsertAsync(TInsert request)
    {
        var entity = MapInsertToEntity(request);

        _set.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<TDto> UpdateAsync(int id, TUpdate request)
    {
        var entity = await FindByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entity not found. Id={id}");

        MapUpdateToEntity(request, entity);

        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await FindByIdAsync(id);
        if (entity == null) return false;

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // Default Find: pretpostavlja da entitet ima int Id
    // Ako ti entiteti nisu int Id, reci odmah pa prilagodimo.
    protected virtual async Task<TEntity?> FindByIdAsync(int id)
    {
        // EF Core FindAsync radi samo kad je ključ PK konfigurisan
        return await _set.FindAsync(id);
    }
}
