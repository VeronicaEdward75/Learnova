using LearnNova.Models.ViewModels;
using LearnNova.Models.ViewModels.Student.Filters;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, 
        BaseFilterParameters filters) where T : class
    {
        var totalCount = await query.CountAsync();

        // Apply Pagination
        int pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        int pageSize = filters.PageSize > 0 ? filters.PageSize : 20;

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
