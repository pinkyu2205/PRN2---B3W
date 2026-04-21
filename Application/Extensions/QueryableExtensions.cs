using Application.DTOs.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<T>
            {
                Items = items,
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static async Task<PagedResult<TDest>> ToPagedResultAsync<TSource, TDest>(this IQueryable<TSource> query, int pageNumber, int pageSize, Expression<Func<TSource, TDest>> selector)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(selector).ToListAsync();
            return new PagedResult<TDest>
            {
                Items = items,
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

    }
}

