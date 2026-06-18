namespace PharmaSaaS.Infrastructure.Extensions;

internal static class QueryableExtensions
{
    private const int _maxIncludeDepth = 2;
    private const int _maxIncludeCount = 3;

    internal static IQueryable<T> ApplyIncludesSafely<T>(this IQueryable<T> query, string[] includes) where T : class
    {
        if (includes is null || includes.Length == 0)
            return query;

        bool forceSplit = includes.Contains(QueryHint.SplitQuery);

        if (forceSplit)
            includes = [.. includes.Where(x => x != QueryHint.SplitQuery)];

        bool hasDeepIncludes = includes.Any(x => x.Count(c => c == '.') >= _maxIncludeDepth);
        bool hasTooManyIncludes = includes.Length >= _maxIncludeCount;

        if (forceSplit || hasDeepIncludes || hasTooManyIncludes)
            query = query.AsSplitQuery();

        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    internal static IQueryable<T> ApplySearchFilter<T>(this IQueryable<T> query, string? searchValue, string? searchColumn, ColumnType searchColumnType)
    {
        if (string.IsNullOrEmpty(searchValue) || string.IsNullOrEmpty(searchColumn))
            return query;

        switch (searchColumnType)
        {
            case ColumnType.String:
                query = query.Where($"{searchColumn}.Contains(@0)", searchValue);
                break;

            case ColumnType.Int:
                if (int.TryParse(searchValue, out var intValue))
                    query = query.Where($"{searchColumn} == @0", intValue);
                break;

            case ColumnType.Bool:
                if (bool.TryParse(searchValue, out var boolValue))
                    query = query.Where($"{searchColumn} == @0", boolValue);
                break;
        }

        return query;
    }

    internal static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, string sortDirection)
        => query.OrderBy($"{sortColumn} {sortDirection}");
}