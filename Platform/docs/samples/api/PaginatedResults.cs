using System.Collections.Generic;
using System.Linq;

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public static class Pagination
{
    // <PaginatedResults>
    public static PaginatedResult<T> Paginate<T>(IQueryable<T> query, int page, int pageSize)
    {
        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedResult<T>(items, page, pageSize, total);
    }
    // </PaginatedResults>
}
