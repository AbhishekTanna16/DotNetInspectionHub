using Microsoft.EntityFrameworkCore;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Application.Helpers;
public static class PaginatedListExtensions
{
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source, int? pageIndex, int? pageSize)
    {
        // No pagination → return full list
        if (pageIndex == null || pageSize == null)
        {
            var allItems = await source.ToListAsync();
            return new PaginatedList<T>(allItems, allItems.Count, 1, allItems.Count);
        }

        int pIndex = pageIndex.Value;
        int pSize = pageSize.Value;

        var count = await source.CountAsync();

        var items = await source.Skip((pIndex - 1) * pSize)
                                .Take(pSize)
                                .ToListAsync();

        return new PaginatedList<T>(items, count, pIndex, pSize);
    }
}
